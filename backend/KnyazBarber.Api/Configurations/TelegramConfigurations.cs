using KnyazBarber.Api.Services.Telegram;
using Telegram.Bot;

namespace KnyazBarber.Api.Configurations;

public static class TelegramConfigurations
{
    /// <summary>Telegram bot. Token bo'lmasa o'chiq: hodisalar navbatga tushmaydi, fon xizmati ishga tushmaydi.</summary>
    public static void AddTelegramConfigurations(this WebApplicationBuilder builder)
    {
        var telegram = builder.Configuration.GetSection(TelegramOptions.Section).Get<TelegramOptions>() ?? new TelegramOptions();
        builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection(TelegramOptions.Section));
        builder.Services.AddSingleton<ITelegramNotifier, TelegramNotifier>();
        builder.Services.AddScoped<ITelegramAccountService, TelegramAccountService>();
        if (!telegram.Enabled) return;

        builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(new TelegramBotClientOptions(telegram.BotToken, telegram.ApiBaseUrl)));
        builder.Services.AddScoped<ITelegramNotificationService, TelegramNotificationService>();
        builder.Services.AddScoped<ITelegramUpdateHandler, TelegramUpdateHandler>();
        builder.Services.AddHostedService<TelegramWorker>();
    }
}
