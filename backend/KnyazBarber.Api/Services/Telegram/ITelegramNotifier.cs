using System.Threading.Channels;

namespace KnyazBarber.Api.Services.Telegram;

public enum BotEventKind { Created, Cancelled }

/// <param name="ActorUserId">Amalni bajargan xodim (unga xabar yuborilmaydi); mijoz saytdan qilgan bo'lsa — null.</param>
public record BotEvent(BotEventKind Kind, int AppointmentId, int? ActorUserId = null);

public interface ITelegramNotifier
{
    ChannelReader<BotEvent> Reader { get; }

    /// <summary>Hodisani navbatga qo'yadi va darhol qaytadi (bot o'chiq bo'lsa — hech narsa qilmaydi).</summary>
    void Publish(BotEvent e);
}
