using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Repositories;

namespace KnyazBarber.Api.Services.Telegram;

/// <summary>
/// Xodim akkauntini Telegram bilan bog'lash. Admin username (yoki raqamli ID) kiritadi;
/// xodim botga birinchi marta yozganda / Mini App'ni ochganda Telegram ID doimiy bog'lanadi.
/// </summary>
public partial class TelegramAccountService(IBaseRepository<User> users, ILogger<TelegramAccountService> logger) : ITelegramAccountService
{
    public async Task<User?> FindOrBindAsync(long telegramId, string? username, CancellationToken ct)
    {
        var user = await users.Query().Include(u => u.Barber).FirstOrDefaultAsync(u => u.TelegramUserId == telegramId, ct);
        if (user is null && !string.IsNullOrEmpty(username))
        {
            var name = username.ToLowerInvariant();
            user = await users.Query().Include(u => u.Barber)
                .FirstOrDefaultAsync(u => u.TelegramUsername == name && u.TelegramUserId == null, ct);
            if (user is not null)
            {
                user.TelegramUserId = telegramId;
                logger.LogInformation("Telegram bog'landi: {Login} ← {TgId} (@{Username})", user.Login, telegramId, name);
                await users.SaveChangesAsync(ct);
            }
        }
        return user is {IsActive: true} ? user : null;
    }

    public Task<List<long>> ChatIdsAsync(int barberId, bool includeAdmins, int? exceptUserId, CancellationToken ct) =>
        users.QueryNoTracking()
            .Where(u => u.IsActive && u.TelegramUserId != null && u.Id != exceptUserId
                        && ((u.Role == UserRole.Barber && u.BarberId == barberId) || (includeAdmins && u.Role == UserRole.Admin)))
            .Select(u => u.TelegramUserId!.Value)
            .Distinct()
            .ToListAsync(ct);

    public Task<List<long>> AdminChatIdsAsync(CancellationToken ct) =>
        users.QueryNoTracking()
            .Where(u => u.IsActive && u.TelegramUserId != null && u.Role == UserRole.Admin)
            .Select(u => u.TelegramUserId!.Value)
            .ToListAsync(ct);

    public async Task ApplyAsync(User user, string? input, CancellationToken ct)
    {
        var value = input?.Trim().TrimStart('@').ToLowerInvariant() ?? "";
        if (value.Length == 0)
        {
            user.TelegramUsername = null;
            user.TelegramUserId = null;
            return;
        }

        if (long.TryParse(value, out var id) && id > 0)
        {
            if (user.TelegramUserId == id) return;
            if (await users.QueryNoTracking().AnyAsync(u => u.Id != user.Id && u.TelegramUserId == id, ct))
                throw new BadRequestException("Bu Telegram ID boshqa akkauntga bog'langan");
            user.TelegramUserId = id;
            user.TelegramUsername = null;
            return;
        }

        if (!UsernameRegex().IsMatch(value))
            throw new BadRequestException("Telegram username noto'g'ri: 5–32 belgi (lotin harf, raqam, _) yoki raqamli ID");
        if (user.TelegramUsername == value) return;
        if (await users.QueryNoTracking().AnyAsync(u => u.Id != user.Id && u.TelegramUsername == value, ct))
            throw new BadRequestException("Bu Telegram username boshqa akkauntda bor");
        // Username o'zgarsa eski bog'lanish o'chadi — yangi odam botga qayta /start yozadi
        user.TelegramUsername = value;
        user.TelegramUserId = null;
    }

    /// <summary>Panelda ko'rsatish uchun: "@username" yoki ID.</summary>
    public static string? Display(User u) => u.TelegramUsername is { } n ? "@" + n : u.TelegramUserId?.ToString();

    [GeneratedRegex("^[a-z][a-z0-9_]{4,31}$")]
    private static partial Regex UsernameRegex();
}
