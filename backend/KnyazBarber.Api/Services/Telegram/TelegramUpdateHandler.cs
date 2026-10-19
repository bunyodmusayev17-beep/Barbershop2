using System.Net;
using Microsoft.Extensions.Options;
using KnyazBarber.Api.Configurations;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TgUser = Telegram.Bot.Types.User;

namespace KnyazBarber.Api.Services.Telegram;

/// <summary>Botga kelgan xabarlar (/start — bog'lash) va navbat tugmalari (keldi/kelmadi/bekor/qo'ng'iroq).</summary>
public class TelegramUpdateHandler(
    IBaseRepository<Appointment> appointments,
    IPublicSiteService publicSite,
    ITelegramBotClient bot,
    ITelegramAccountService accounts,
    ITelegramNotificationService notifications,
    ITelegramNotifier notifier,
    BusinessClock clock,
    IOptions<TelegramOptions> options,
    ILogger<TelegramUpdateHandler> logger) : ITelegramUpdateHandler
{
    public Task HandleAsync(Update update, CancellationToken ct) => update switch
    {
        {CallbackQuery: { } q} => OnCallbackAsync(q, ct),
        {Message: {From: { } from, Chat.Type: ChatType.Private} m} => OnMessageAsync(m, from, ct),
        _ => Task.CompletedTask,
    };

    private async Task OnMessageAsync(Message m, TgUser from, CancellationToken ct)
    {
        var staff = await accounts.FindOrBindAsync(from.Id, from.Username, ct);
        logger.LogInformation("Bot xabar: {TgId} @{Username} \"{Text}\" → {Who}",
            from.Id, from.Username ?? "-", Short(m.Text), staff is null ? "mijoz/notanish" : $"xodim {staff.Login}");
        string text;
        if (staff is not null)
        {
            var role = staff.Role == UserRole.Admin ? "admin" : $"usta ({WebUtility.HtmlEncode(staff.Barber?.Name)})";
            text = $"Salom! Siz <b>{staff.Login}</b> — {role} sifatida ulangansiz ✅\n\n"
                   + (staff.Role == UserRole.Admin
                       ? "Yangi navbatlar va kechki hisobot shu yerga keladi."
                       : "Yangi navbatlaringiz va 2 soat oldin eslatma shu yerga keladi.")
                   + "\nPanelni pastdagi <b>Ochish</b> tugmasi orqali ochasiz.";
        }
        else
        {
            // /manzil va /aloqa — saytdagi "Manzil" va "Qo'ng'iroq" kabi
            var command = m.Text?.Split(' ', '@')[0].ToLowerInvariant();
            if (command is "/manzil" or "/location")
            {
                await SendLocationAsync(m.Chat.Id, ct);
                return;
            }
            if (command is "/aloqa" or "/contact")
            {
                await SendPhoneAsync(m.Chat.Id, ct);
                return;
            }

            // Mijoz uchun xabar saytdagi ma'lumotdan: admin panelda o'zgartirilsa bot ham yangisini yozadi
            var lang = TelegramWelcome.Lang(from.LanguageCode);
            var site = await publicSite.GetSiteAsync(ct);
            text = TelegramWelcome.Text(site.Settings, lang)
                   + $"\n\n<i>Xodim bo'lsangiz, adminga Telegram ID'ingizni bering: <code>{from.Id}</code></i>";
            await bot.SendMessage(m.Chat.Id, text, parseMode: ParseMode.Html,
                replyMarkup: TelegramWelcome.Keyboard(options.Value.MiniAppUrl, lang), cancellationToken: ct);
            return;
        }

        var url = options.Value.MiniAppUrl;
        var keyboard = string.IsNullOrEmpty(url) ? null : new InlineKeyboardMarkup(InlineKeyboardButton.WithWebApp("Ochish", new WebAppInfo(url)));
        await bot.SendMessage(m.Chat.Id, text, parseMode: ParseMode.Html, replyMarkup: keyboard, cancellationToken: ct);
    }

    private async Task SendLocationAsync(long chatId, CancellationToken ct)
    {
        var s = (await publicSite.GetSiteAsync(ct)).Settings;
        await bot.SendVenue(chatId, s.Latitude, s.Longitude, s.BusinessName, s.Address.Uz, cancellationToken: ct);
    }

    private async Task SendPhoneAsync(long chatId, CancellationToken ct)
    {
        var s = (await publicSite.GetSiteAsync(ct)).Settings;
        await bot.SendContact(chatId, s.Phone.Replace(" ", ""), s.BusinessName, cancellationToken: ct);
    }

    private async Task OnCallbackAsync(CallbackQuery q, CancellationToken ct)
    {
        try
        {
            var result = await HandleCallbackAsync(q, ct);
            logger.LogInformation("Bot tugma: {TgId} @{Username} \"{Data}\" → {Result}", q.From.Id, q.From.Username ?? "-", q.Data, result);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            // Javob berilmasa tugmada "yuklanmoqda" belgisi qotib qoladi — xatoda ham albatta javob beramiz
            logger.LogError(ex, "Bot tugma xatosi: \"{Data}\"", q.Data);
            try { await Answer(q, "Xatolik yuz berdi, birozdan keyin qayta urinib ko'ring", CancellationToken.None, alert: true); }
            catch (Exception answerEx) { logger.LogWarning("Tugmaga javob berilmadi: {Error}", answerEx.Message); }
        }
    }

    /// <summary>Tugmani bajaradi va log uchun natijani qaytaradi.</summary>
    private async Task<string> HandleCallbackAsync(CallbackQuery q, CancellationToken ct)
    {
        // Mijoz tugmalari (manzil, qo'ng'iroq) — xodim bo'lishi shart emas
        if (q.Data is TelegramWelcome.Map or TelegramWelcome.Call && q.Message is { } infoMsg)
        {
            await Answer(q, null, ct);
            if (q.Data == TelegramWelcome.Map) await SendLocationAsync(infoMsg.Chat.Id, ct);
            else await SendPhoneAsync(infoMsg.Chat.Id, ct);
            return $"mijoz: {q.Data}";
        }

        var user = await accounts.FindOrBindAsync(q.From.Id, q.From.Username, ct);
        if (user is null || q.Message is not { } msg || !TelegramMessages.TryParseCallback(q.Data, out var id, out var action))
        {
            await Answer(q, "Ruxsat yo'q", ct);
            return user is null ? "rad etildi: xodim emas" : "rad etildi: noto'g'ri tugma";
        }

        var a = await notifications.LoadAsync(id, ct);
        if (a is null)
        {
            await Answer(q, "Navbat topilmadi", ct);
            return $"{user.Login}: #{id} topilmadi";
        }
        // Usta faqat o'z navbatlarini boshqaradi
        if (user.Role != UserRole.Admin && a.BarberId != user.BarberId)
        {
            await Answer(q, "Bu navbat sizniki emas", ct);
            return $"{user.Login}: #{a.Id} rad etildi (boshqa ustaning navbati)";
        }

        switch (action)
        {
            case TelegramMessages.Call:
                await bot.SendContact(msg.Chat.Id, a.ClientPhone, a.ClientName, cancellationToken: ct);
                await Answer(q, null, ct);
                return $"{user.Login}: #{a.Id} mijoz kontakti yuborildi";

            case TelegramMessages.Cancel when !TelegramMessages.IsFinal(a.Status):
                await IgnoreNotModified(bot.EditMessageReplyMarkup(msg.Chat.Id, msg.Id, TelegramMessages.CancelConfirmKeyboard(a.Id), cancellationToken: ct));
                await Answer(q, null, ct);
                return $"{user.Login}: #{a.Id} bekor qilish tasdig'i so'raldi";

            case TelegramMessages.CancelNo:
                await IgnoreNotModified(bot.EditMessageReplyMarkup(msg.Chat.Id, msg.Id, TelegramMessages.Keyboard(a.Id, a.Status), cancellationToken: ct));
                await Answer(q, null, ct);
                return $"{user.Login}: #{a.Id} bekor qilinmadi";
        }

        var target = action switch
        {
            TelegramMessages.Done => AppointmentStatus.Completed,
            TelegramMessages.NoShow => AppointmentStatus.NoShow,
            TelegramMessages.Cancel or TelegramMessages.CancelYes => AppointmentStatus.Cancelled,
            _ => (AppointmentStatus?)null,
        };
        if (target is null)
        {
            await Answer(q, null, ct);
            return $"{user.Login}: #{a.Id} noma'lum amal";
        }

        if (TelegramMessages.IsFinal(a.Status))
        {
            await Answer(q, $"Holati allaqachon: {TelegramMessages.StatusLabel(a.Status)}", ct, alert: true);
            await RefreshAsync(msg, a, null, ct);
            return $"{user.Login}: #{a.Id} holati allaqachon {a.Status}";
        }
        if (target != AppointmentStatus.Cancelled && a.StartUtc > clock.UtcNow)
        {
            await Answer(q, "Hali vaqti kelmagan — navbat boshlangandan keyin belgilang", ct, alert: true);
            return $"{user.Login}: #{a.Id} hali vaqti kelmagan";
        }

        a.Status = target.Value;
        await appointments.SaveChangesAsync(ct);
        if (target == AppointmentStatus.Cancelled) notifier.Publish(new BotEvent(BotEventKind.Cancelled, a.Id, user.Id));

        await RefreshAsync(msg, a, $"{user.Login}, {clock.ToLocal(clock.UtcNow):HH:mm}", ct);
        await Answer(q, TelegramMessages.StatusLabel(a.Status), ct);
        return $"{user.Login}: #{a.Id} → {a.Status}";
    }

    /// <summary>Log uchun matnni qisqartirish (uzun xabarlar logni to'ldirmasin).</summary>
    private static string Short(string? s) => s is null ? "" : s.Length <= 60 ? s : s[..60] + "…";

    private Task RefreshAsync(Message msg, Appointment a, string? footer, CancellationToken ct) =>
        IgnoreNotModified(bot.EditMessageText(msg.Chat.Id, msg.Id, TelegramMessages.Card(notifications.ToCard(a), CardKind.Updated, footer),
            parseMode: ParseMode.Html, replyMarkup: TelegramMessages.Keyboard(a.Id, a.Status), cancellationToken: ct));

    /// <summary>Tugma ikki marta bosilsa Telegram "message is not modified" qaytaradi — bu xato emas.</summary>
    private static async Task IgnoreNotModified(Task edit)
    {
        try { await edit; }
        catch (ApiRequestException ex) when (ex.Message.Contains("message is not modified", StringComparison.OrdinalIgnoreCase)) { }
    }

    private Task Answer(CallbackQuery q, string? text, CancellationToken ct, bool alert = false) =>
        bot.AnswerCallbackQuery(q.Id, text, showAlert: alert, cancellationToken: ct);
}
