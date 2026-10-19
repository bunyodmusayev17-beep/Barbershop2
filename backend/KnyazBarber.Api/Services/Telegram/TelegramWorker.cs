using Microsoft.Extensions.Options;
using KnyazBarber.Api.Configurations;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace KnyazBarber.Api.Services.Telegram;

/// <summary>
/// Botning fon ishlari (faqat token sozlanganda ishga tushadi):
/// 1) long polling — xabar va tugmalarni qabul qilish (public URL kerak emas);
/// 2) navbat hodisalari navbatidan xabar yuborish;
/// 3) har daqiqada — ustalarga eslatma va kechki hisobot.
/// </summary>
public class TelegramWorker(
    ITelegramBotClient bot,
    ITelegramNotifier notifier,
    IServiceScopeFactory scopes,
    IOptions<TelegramOptions> options,
    ILogger<TelegramWorker> logger) : BackgroundService
{
    /// <summary>Bitta xabar/tugma yoki hodisaga ajratilgan eng ko'p vaqt — biri osilib qolsa, qolganlari kutib qolmaydi.</summary>
    private static readonly TimeSpan WorkTimeout = TimeSpan.FromSeconds(30);

    protected override Task ExecuteAsync(CancellationToken ct) =>
        Task.WhenAll(SetupAsync(ct), PollAsync(ct), SendEventsAsync(ct), ScheduleAsync(ct));

    /// <summary>
    /// Bot buyruqlari va "Ochish" (Mini App) menyu tugmasi — BotFather'da qo'lda sozlash shart emas.
    /// Server ishga tushganda internet bo'lmasa — har daqiqada qayta urinadi (polling bunga qaramay ishlayveradi).
    /// </summary>
    private async Task SetupAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var me = await bot.GetMe(ct);
                await bot.DeleteWebhook(cancellationToken: ct);
                await bot.SetMyCommands([
                    new BotCommand {Command = "start", Description = "Bosh sahifa va navbat olish"},
                    new BotCommand {Command = "manzil", Description = "Manzil va xarita"},
                    new BotCommand {Command = "aloqa", Description = "Telefon raqami"},
                ], cancellationToken: ct);
                await bot.SetMyCommands([
                    new BotCommand {Command = "start", Description = "Главная и онлайн-запись"},
                    new BotCommand {Command = "manzil", Description = "Адрес и карта"},
                    new BotCommand {Command = "aloqa", Description = "Номер телефона"},
                ], languageCode: "ru", cancellationToken: ct);
                var url = options.Value.MiniAppUrl;
                if (!string.IsNullOrEmpty(url))
                    await bot.SetChatMenuButton(menuButton: new MenuButtonWebApp {Text = "Ochish", WebApp = new WebAppInfo(url)}, cancellationToken: ct);
                logger.LogInformation("Telegram bot ishga tushdi: @{Username}", me.Username);
                return;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError("Telegram bot sozlanmadi (token to'g'rimi? internet bormi?): {Error} — 1 daqiqadan keyin qayta uriniladi", ex.Message);
                await Delay(TimeSpan.FromMinutes(1), ct);
            }
        }
    }

    private async Task PollAsync(CancellationToken ct)
    {
        int? offset = null;
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var updates = await bot.GetUpdates(offset, timeout: 30,
                    allowedUpdates: [UpdateType.Message, UpdateType.CallbackQuery], cancellationToken: ct);
                foreach (var update in updates)
                {
                    offset = update.Id + 1;
                    await RunScopedAsync("update", (sp, t) => sp.GetRequiredService<ITelegramUpdateHandler>().HandleAsync(update, t), ct);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Telegram polling xatosi: {Error}", ex.Message);
                await Delay(TimeSpan.FromSeconds(5), ct);
            }
        }
    }

    private async Task SendEventsAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var e in notifier.Reader.ReadAllAsync(ct))
                await RunScopedAsync("event", (sp, t) => sp.GetRequiredService<ITelegramNotificationService>().HandleEventAsync(e, t), ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }

    private async Task ScheduleAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            await RunScopedAsync("reminders", (sp, t) => sp.GetRequiredService<ITelegramNotificationService>().SendRemindersAsync(t), ct);
            await RunScopedAsync("report", (sp, t) => sp.GetRequiredService<ITelegramNotificationService>().SendDailyReportIfDueAsync(t), ct);
        } while (await Tick(timer, ct));
    }

    /// <summary>Har bir ish alohida DI scope'da (DbContext) — bittasining xatosi boshqalarini to'xtatmaydi.</summary>
    private async Task RunScopedAsync(string name, Func<IServiceProvider, CancellationToken, Task> work, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(WorkTimeout);
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            await work(scope.ServiceProvider, timeout.Token);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Telegram: {Work} {Seconds} soniyada tugamadi — to'xtatildi", name, WorkTimeout.TotalSeconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Telegram: {Work} bajarilmadi", name);
        }
    }

    private static async Task<bool> Tick(PeriodicTimer timer, CancellationToken ct)
    {
        try { return await timer.WaitForNextTickAsync(ct); }
        catch (OperationCanceledException) { return false; }
    }

    private static async Task Delay(TimeSpan delay, CancellationToken ct)
    {
        try { await Task.Delay(delay, ct); }
        catch (OperationCanceledException) { }
    }
}
