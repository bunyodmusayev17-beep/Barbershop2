using KnyazBarber.Api.Dtos.Profile;

namespace KnyazBarber.Api.Services;

/// <summary>Usta o'z profilini boshqaradi: rasmi va ish rasmlari. Har bir metod faqat shu ustaning ma'lumotiga tegadi.</summary>
public interface IBarberProfileService
{
    Task<BarberProfileDto> GetAsync(int barberId, CancellationToken ct);
    Task<BarberProfileDto> UploadPhotoAsync(int barberId, IFormFile file, CancellationToken ct);
    Task<BarberProfileDto> DeletePhotoAsync(int barberId, CancellationToken ct);
    Task<BarberProfileDto> UploadWorksAsync(int barberId, List<IFormFile> files, CancellationToken ct);
    Task<BarberProfileDto> DeleteWorkAsync(int barberId, int workId, CancellationToken ct);
}
