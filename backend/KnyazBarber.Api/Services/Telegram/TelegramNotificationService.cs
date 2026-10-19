using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using KnyazBarber.Api.Configurations;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace KnyazBarber.Api.Services.Telegram;

/// <summary>Bot yuboradigan xabarlar: yangi/bekor qilingan navbat, ustaga eslatma, kechki hisobot.</summary>
public class TelegramNotificationService(
    IBaseRepository<Appointment> appointments,
    IBaseRepository<Barber> barbers,
    IBaseRepository<SiteSettings> settings,
    ITelegramBotClient bot,
    ITelegramAccountService accounts,
    BusinessClock clock,
    IOptions<TelegramOptions> options,
    ILogger<TelegramNotificationService> logger) : ITelegramNotificationService
{
    public async Task HandleEventAsync(BotEvent e, CancellationToken ct)
    {
        var a = await LoadAsync(e.AppointmentId, ct);
        if (a is null) return;

        var created = e.Kind == BotEventKind.Created;
        // Yangi navbat — adminlar va usta; bekor qilish — faqat usta (admin o'zi bekor qilgan bo'ladi yoki panelda ko'radi)
        var chats = await accounts.ChatIdsAsync(a.BarberId, includeAdmins: created, e.ActorUserId, ct);
        var text = TelegramMessages.Card(ToCard(a), created ? CardKind.New : CardKind.Cancelled);
        await SendAsync(created ? $"yangi navbat #{a.Id}" : $"bekor qilindi #{a.Id}", chats, text, TelegramMessages.Keyboard(a.Id, a.Status), ct);
    }

    /// <summary>Shu oyna ichida yaratilgan navbatga eslatma yuborilmaydi — yangi navbat xabari yetarli.</summary>
    public async Task SendRemindersAsync(CancellationToken ct)
    {
        var lead = options.Value.ReminderLead;
        var now = clock.UtcNow;
        var until = now + lead;
        var due = await appointments.Query().Include(a => a.Barber).Include(a => a.Service)
            .Where(a => a.ReminderSentAt == null && a.StartUtc > now && a.StartUtc <= until
                        && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed))
            .ToListAsync(ct);
        if (due.Count == 0) return;

        foreach (var a in due)
        {
            a.ReminderSentAt = now;
            if (a.CreatedAt > a.StartUtc - lead) continue;
            var chats = await accounts.ChatIdsAsync(a.BarberId, includeAdmins: false, exceptUserId: null, ct);
            await SendAsync($"eslatma #{a.Id}", chats, TelegramMessages.Card(ToCard(a), CardKind.Reminder), TelegramMessages.Keyboard(a.Id, a.Status), ct);
        }
        await appointments.SaveChangesAsync(ct);
    }

    /// <summary>Kecha ham tekshiriladi: ish yarim tundan keyin tugasa hisobot ertasi kuni yuboriladi.</summary>
    public async Task SendDailyReportIfDueAsync(CancellationToken ct)
    {
        var site = await settings.GetByIdAsync(1, ct);
        if (site is null) return;

        var today = clock.Today;
        var nowLocal = clock.ToLocal(clock.UtcNow).DateTime;
        // Birinchi ishga tushishda o'tgan kunlar uchun hisobot yuborilmaydi
        var last = site.LastReportDate ?? today.AddDays(-1);

        foreach (var date in new[] {today.AddDays(-1), today})
        {
            if (date <= last) continue;
            var hours = await barbers.QueryNoTracking().Where(b => b.IsActive)
                .SelectMany(b => b.WorkingHours)
                .Where(h => h.DayOfWeek == date.DayOfWeek)
                .Select(h => new {h.End, h.IsDayOff})
                .ToListAsync(ct);
            var at = TelegramMessages.ReportTime(date, hours.Select(h => (h.End, h.IsDayOff)));
            if (at is null || nowLocal < at) continue;

            var (start, end) = clock.DayRangeUtc(date);
            var rows = await appointments.QueryNoTracking().Where(a => a.StartUtc >= start && a.StartUtc < end)
                .Select(a => new ReportRow(a.Barber.Name, a.Status, a.Price))
                .ToListAsync(ct);
            await SendAsync($"kechki hisobot {date:dd.MM}", await accounts.AdminChatIdsAsync(ct), TelegramMessages.DailyReport(date, rows), null, ct);

            site.LastReportDate = date;
            last = date;
            await settings.SaveChangesAsync(ct);
        }
    }

    public Task<Appointment?> LoadAsync(int id, CancellationToken ct) =>
        appointments.GetByIdAsync(id, ct, a => a.Barber, a => a.Service);

    public AppointmentCard ToCard(Appointment a) => new(
        a.Id, a.ClientName, a.ClientPhone, a.Service.Name.Uz, a.Barber.Name,
        clock.ToLocal(a.StartUtc).DateTime, clock.ToLocal(a.EndUtc).DateTime, a.Price, a.Comment, a.Status);

    private async Task SendAsync(string what, IReadOnlyCollection<long> chats, string text, InlineKeyboardMarkup? keyboard, CancellationToken ct)
    {
        var sent = 0;
        foreach (var chat in chats)
        {
            try
            {
                await bot.SendMessage(chat, text, parseMode: ParseMode.Html, replyMarkup: keyboard, cancellationToken: ct);
                sent++;
            }
            catch (RequestException ex)
            {
                // Xodim botni bloklagan yoki tarmoq xatosi — boshqalarga yuborish davom etadi
                logger.LogWarning("Telegram: {Chat} ga yuborilmadi: {Error}", chat, ex.Message);
            }
        }
        logger.LogInformation("Bot yubordi: {What} → {Sent}/{Total} chat", what, sent, chats.Count);
    }
}
