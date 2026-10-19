using Telegram.Bot.Types;

namespace KnyazBarber.Api.Services.Telegram;

public interface ITelegramUpdateHandler
{
    /// <summary>Botga kelgan xabar yoki tugma bosilishi.</summary>
    Task HandleAsync(Update update, CancellationToken ct);
}
