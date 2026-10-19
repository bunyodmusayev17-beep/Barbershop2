using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Dtos.Profile;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Repositories;

namespace KnyazBarber.Api.Services;

public class BarberProfileService(
    IBaseRepository<Barber> barbers,
    IBaseRepository<GalleryImage> images,
    IBarberManagementService management,
    IGalleryService gallery) : IBarberProfileService
{
    /// <summary>Bitta usta nechtagacha ish rasmi yuklay oladi.</summary>
    public const int MaxWorks = 30;

    public async Task<BarberProfileDto> GetAsync(int barberId, CancellationToken ct)
    {
        var b = await barbers.QueryNoTracking().Where(x => x.Id == barberId)
            .Select(x => new {x.Id, x.Name, x.PhotoUrl})
            .FirstOrDefaultAsync(ct) ?? throw new NotFoundException("Usta topilmadi");
        // Ustaga eng yangi ishlari tepada ko'rinadi
        var works = await images.QueryNoTracking().Where(g => g.BarberId == barberId).OrderByDescending(g => g.Id)
            .Select(g => new BarberWorkDto(g.Id, g.Url, g.CreatedAt)).ToListAsync(ct);
        return new BarberProfileDto(b.Id, b.Name, b.PhotoUrl, works, MaxWorks);
    }

    public async Task<BarberProfileDto> UploadPhotoAsync(int barberId, IFormFile file, CancellationToken ct)
    {
        await management.UploadPhotoAsync(barberId, file, ct);
        return await GetAsync(barberId, ct);
    }

    public async Task<BarberProfileDto> DeletePhotoAsync(int barberId, CancellationToken ct)
    {
        await management.DeletePhotoAsync(barberId, ct);
        return await GetAsync(barberId, ct);
    }

    public async Task<BarberProfileDto> UploadWorksAsync(int barberId, List<IFormFile> files, CancellationToken ct)
    {
        var count = await images.QueryNoTracking().CountAsync(g => g.BarberId == barberId, ct);
        if (count + files.Count > MaxWorks)
            throw new BadRequestException($"Ko'pi bilan {MaxWorks} ta ish rasmi. Yana {Math.Max(0, MaxWorks - count)} ta yuklash mumkin", "works_limit");
        await gallery.UploadAsync(files, barberId, ct);
        return await GetAsync(barberId, ct);
    }

    public async Task<BarberProfileDto> DeleteWorkAsync(int barberId, int workId, CancellationToken ct)
    {
        // Boshqa ustaning rasmi bo'lsa ham "topilmadi" — borligini oshkor qilmaymiz
        if (!await images.QueryNoTracking().AnyAsync(g => g.Id == workId && g.BarberId == barberId, ct))
            throw new NotFoundException("Rasm topilmadi");
        await gallery.DeleteAsync(workId, ct);
        return await GetAsync(barberId, ct);
    }
}
