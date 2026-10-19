using KnyazBarber.Api.Dtos.Gallery;

namespace KnyazBarber.Api.Services;

public interface IGalleryService
{
    Task<List<GalleryAdminDto>> ListAsync(CancellationToken ct);
    /// <summary>Bir nechta rasmni bir vaqtda yuklash (1–10 ta).</summary>
    Task<List<GalleryAdminDto>> UploadAsync(List<IFormFile> files, CancellationToken ct);
    Task UpdateAsync(int id, GalleryUpdate input, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
