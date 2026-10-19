using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Dtos.User;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;
using KnyazBarber.Api.Services.Telegram;

namespace KnyazBarber.Api.Services;

public class UserService(
    IBaseRepository<User> users,
    IBaseRepository<Barber> barbers,
    ITelegramAccountService telegram) : IUserService
{
    private static readonly PasswordHasher<User> Hasher = new();

    public async Task<List<UserAdminDto>> ListAsync(CancellationToken ct) =>
        (await users.QueryNoTracking().Include(u => u.Barber).OrderBy(u => u.Role).ThenBy(u => u.Login).ToListAsync(ct))
        .Select(u => u.ToAdminDto()).ToList();

    public async Task<UserAdminDto> CreateAsync(UserCreate input, CancellationToken ct)
    {
        var login = input.Login.Trim().ToLowerInvariant();
        if (await users.QueryNoTracking().AnyAsync(u => u.Login == login, ct)) throw new ConflictException("Bu login band");
        await ValidateBarberAsync(input.Role, input.BarberId, ct);

        var user = new User
        {
            Login = login,
            Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim(),
            Role = input.Role,
            BarberId = input.Role == UserRole.Barber ? input.BarberId : null,
        };
        await telegram.ApplyAsync(user, input.Telegram, ct);
        user.PasswordHash = Hasher.HashPassword(user, input.Password);
        await users.AddAsync(user, ct);
        return (await LoadAsync(user.Id, ct)).ToAdminDto();
    }

    public async Task<UserAdminDto> UpdateAsync(int id, UserUpdate input, int? currentUserId, CancellationToken ct)
    {
        var user = await LoadAsync(id, ct);
        await ValidateBarberAsync(input.Role, input.BarberId, ct);

        // O'zini admin rolidan chiqarib yoki o'chirib qo'yib, tizimni adminsiz qoldirmaslik
        if (currentUserId == id && (input.Role != UserRole.Admin || !input.IsActive))
            throw new BadRequestException("O'z akkauntingizni adminlikdan chiqarib yoki o'chirib bo'lmaydi");

        user.Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();
        user.Role = input.Role;
        user.BarberId = input.Role == UserRole.Barber ? input.BarberId : null;
        user.IsActive = input.IsActive;
        if (!string.IsNullOrEmpty(input.NewPassword)) user.PasswordHash = Hasher.HashPassword(user, input.NewPassword);
        await telegram.ApplyAsync(user, input.Telegram, ct);
        await users.SaveChangesAsync(ct);
        return (await LoadAsync(id, ct)).ToAdminDto();
    }

    public async Task DeleteAsync(int id, int? currentUserId, CancellationToken ct)
    {
        if (currentUserId == id) throw new BadRequestException("O'z akkauntingizni o'chirib bo'lmaydi");
        var user = await users.GetByIdAsync(id, ct) ?? throw new NotFoundException("Foydalanuvchi topilmadi");
        await users.DeleteAsync(user, ct);
    }

    /// <summary>Usta biriktirilgan bo'lsa — uning nomi ham kerak (BarberId o'zgarsa qayta yuklanadi).</summary>
    private async Task<User> LoadAsync(int id, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(id, ct) ?? throw new NotFoundException("Foydalanuvchi topilmadi");
        user.Barber = user.BarberId is { } bid ? await barbers.GetByIdAsync(bid, ct) : null;
        return user;
    }

    private async Task ValidateBarberAsync(UserRole role, int? barberId, CancellationToken ct)
    {
        if (role != UserRole.Barber) return;
        if (barberId is null) throw new BadRequestException("Usta akkaunti uchun ustani tanlang");
        if (!await barbers.QueryNoTracking().AnyAsync(b => b.Id == barberId, ct)) throw new BadRequestException("Usta topilmadi");
    }
}
