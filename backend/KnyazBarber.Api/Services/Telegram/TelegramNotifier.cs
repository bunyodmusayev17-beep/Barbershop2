using System.Threading.Channels;
using Microsoft.Extensions.Options;
using KnyazBarber.Api.Configurations;

namespace KnyazBarber.Api.Services.Telegram;

/// <summary>
/// Navbat hodisalari kanali. Publish darhol qaytadi — Telegram sekin ishlasa yoki xato bersa ham
/// navbat yaratish/bekor qilish so'rovi kutmaydi va buzilmaydi. Xabarlarni <see cref="TelegramWorker"/> yuboradi.
/// </summary>
public class TelegramNotifier(IOptions<TelegramOptions> options) : ITelegramNotifier
{
    private readonly Channel<BotEvent> _channel = Channel.CreateBounded<BotEvent>(
        new BoundedChannelOptions(1000) {FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true});

    public ChannelReader<BotEvent> Reader => _channel.Reader;

    public void Publish(BotEvent e)
    {
        if (options.Value.Enabled) _channel.Writer.TryWrite(e);
    }
}
