using KnyazBarber.Api.Dtos.Barber;

namespace KnyazBarber.Api.Services;

/// <summary>Ustalar: ma'lumot, xizmatlari, haftalik jadvali, rasmi va dam olish vaqtlari (admin panel).</summary>
public interface IBarberManagementService
{
    Task<List<BarberAdminDto>> ListAsync(CancellationToken ct);
    Task<BarberAdminDto> CreateAsync(BarberInput input, CancellationToken ct);
    Task<BarberAdminDto> UpdateAsync(int id, BarberInput input, CancellationToken ct);
    /// <summary>Navbatlari bor usta o'chirilmaydi — nofaol qilinadi.</summary>
    Task DeleteAsync(int id, CancellationToken ct);
    Task<BarberAdminDto> UploadPhotoAsync(int id, IFormFile file, CancellationToken ct);
    Task<BarberAdminDto> DeletePhotoAsync(int id, CancellationToken ct);
    Task<BarberAdminDto> AddTimeOffAsync(int id, TimeOffInput input, CancellationToken ct);
    Task<BarberAdminDto> DeleteTimeOffAsync(int id, int timeOffId, CancellationToken ct);
}
