using KnyazBarber.Api.Dtos.Auth;

namespace KnyazBarber.Api.Services;

public interface IAuthService
{
    /// <summary>Login + parol. Noto'g'ri bo'lsa — UnauthorizedException (sababi faqat logda).</summary>
    Task<LoginResponse> LoginAsync(LoginRequest req, string ip, CancellationToken ct);

    /// <summary>
    /// Mini App'dan parolsiz kirish: Telegram imzolagan initData tekshiriladi. Xodim bo'lsa — token,
    /// oddiy mijoz bo'lsa yoki bot o'chiq bo'lsa — null (sayt ommaviy rejimda qoladi).
    /// </summary>
    Task<LoginResponse?> TelegramLoginAsync(TelegramLoginRequest req, string ip, CancellationToken ct);

    Task<UserDto> MeAsync(int userId, CancellationToken ct);
}
