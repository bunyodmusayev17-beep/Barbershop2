using KnyazBarber.Api.Dtos.User;

namespace KnyazBarber.Api.Services;

/// <summary>Xodim akkauntlari (admin va ustalar).</summary>
public interface IUserService
{
    Task<List<UserAdminDto>> ListAsync(CancellationToken ct);
    Task<UserAdminDto> CreateAsync(UserCreate input, CancellationToken ct);
    /// <param name="currentUserId">So'rovni yuborgan admin — o'zini adminlikdan chiqara yoki o'chira olmaydi.</param>
    Task<UserAdminDto> UpdateAsync(int id, UserUpdate input, int? currentUserId, CancellationToken ct);
    Task DeleteAsync(int id, int? currentUserId, CancellationToken ct);
}
