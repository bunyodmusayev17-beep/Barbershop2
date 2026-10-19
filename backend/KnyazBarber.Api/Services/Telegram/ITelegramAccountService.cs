using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Services.Telegram;

public interface ITelegramAccountService
{
    /// <summary>Telegram ID bo'yicha faol xodimni topadi; topilmasa — username bo'yicha topib ID'ni bog'laydi.</summary>
    Task<User?> FindOrBindAsync(long telegramId, string? username, CancellationToken ct);

    /// <summary>Navbat haqida xabar oladigan chatlar: o'sha usta (+ adminlar), amalni bajargan odamdan tashqari.</summary>
    Task<List<long>> ChatIdsAsync(int barberId, bool includeAdmins, int? exceptUserId, CancellationToken ct);

    Task<List<long>> AdminChatIdsAsync(CancellationToken ct);

    /// <summary>
    /// Admin kiritgan qiymatni foydalanuvchiga yozadi: "@username", "username" yoki raqamli ID; bo'sh — uzish.
    /// Xato bo'lsa — BadRequestException. Saqlamaydi.
    /// </summary>
    Task ApplyAsync(User user, string? input, CancellationToken ct);
}
