using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using KnyazBarber.Api.Configurations;
using KnyazBarber.Api.Dtos.Auth;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;
using KnyazBarber.Api.Services.Telegram;

namespace KnyazBarber.Api.Services;

public class AuthService(
    IBaseRepository<User> users,
    ITokenService tokens,
    ITelegramAccountService accounts,
    IOptions<TelegramOptions> telegram,
    TimeProvider time,
    ILogger<AuthService> logger) : IAuthService
{
    private const string InvalidCredentials = "Login yoki parol noto'g'ri";
    private static readonly PasswordHasher<User> Hasher = new();

    public async Task<LoginResponse> LoginAsync(LoginRequest req, string ip, CancellationToken ct)
    {
        var login = req.Login.Trim().ToLowerInvariant();
        var user = await users.Query().Include(u => u.Barber).FirstOrDefaultAsync(u => u.Login == login, ct);

        // Login topilmasa ham, parol noto'g'ri bo'lsa ham bir xil javob (logda — sababi, parol hech qachon yozilmaydi)
        if (user is null || !user.IsActive)
        {
            logger.LogWarning("Kirish xato: \"{Login}\" ({Reason}), IP {Ip}", Short(login), user is null ? "login yo'q" : "bloklangan", ip);
            throw new UnauthorizedException(InvalidCredentials);
        }

        var result = Hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Kirish xato: \"{Login}\" (parol noto'g'ri), IP {Ip}", user.Login, ip);
            throw new UnauthorizedException(InvalidCredentials);
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = Hasher.HashPassword(user, req.Password);
            await users.SaveChangesAsync(ct);
        }

        logger.LogInformation("Kirish: {Login} ({Role}) parol bilan, IP {Ip}", user.Login, user.Role, ip);
        return Issue(user);
    }

    public async Task<LoginResponse?> TelegramLoginAsync(TelegramLoginRequest req, string ip, CancellationToken ct)
    {
        if (!telegram.Value.Enabled) return null;
        var tg = TelegramInitData.Validate(req.InitData, telegram.Value.BotToken, time.GetUtcNow());
        if (tg is null)
        {
            logger.LogWarning("Mini App: yaroqsiz Telegram imzosi, IP {Ip}", ip);
            throw new UnauthorizedException("Telegram ma'lumoti yaroqsiz");
        }

        var user = await accounts.FindOrBindAsync(tg.Id, tg.Username, ct);
        logger.LogInformation("Mini App ochildi: {TgId} @{Username} → {Who}", tg.Id, tg.Username ?? "-", user is null ? "mijoz" : $"xodim {user.Login}");
        return user is null ? null : Issue(user);
    }

    public async Task<UserDto> MeAsync(int userId, CancellationToken ct)
    {
        var user = await users.QueryNoTracking().Include(u => u.Barber).FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, ct);
        return user?.ToDto() ?? throw new UnauthorizedException("Sessiya tugagan");
    }

    private LoginResponse Issue(User user)
    {
        var (token, expires) = tokens.Create(user);
        return new LoginResponse(token, expires, user.ToDto());
    }

    private static string Short(string s) => s.Length <= 30 ? s : s[..30] + "…";
}
