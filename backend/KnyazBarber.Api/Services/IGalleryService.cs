using KnyazBarber.Api.Dtos.Gallery;

namespace KnyazBarber.Api.Services;

public interface IGalleryService
{
    Task<List<GalleryAdminDto>> ListAsync(CancellationToken ct);
    /// <summary>Bir nechta rasmni bir vaqtda yuklash (1–10 ta). barberId — usta o'z ishini yuklaganda, admin uchun null.</summary>
    Task<List<GalleryAdminDto>> UploadAsync(List<IFormFile> files, int? barberId, CancellationToken ct);
    Task UpdateAsync(int id, GalleryUpdate input, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
