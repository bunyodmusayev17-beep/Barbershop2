using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Dtos.Gallery;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;

namespace KnyazBarber.Api.Services;

public class GalleryService(IBaseRepository<GalleryImage> images, IFileUploadService files) : IGalleryService
{
    public Task<List<GalleryAdminDto>> ListAsync(CancellationToken ct) =>
        images.QueryNoTracking().OrderBy(g => g.SortOrder).ThenBy(g => g.Id)
            .Select(g => new GalleryAdminDto(g.Id, g.Url, g.Caption, g.SortOrder)).ToListAsync(ct);

    public async Task<List<GalleryAdminDto>> UploadAsync(List<IFormFile> uploads, CancellationToken ct)
    {
        if (uploads.Count is 0 or > 10) throw new BadRequestException("1 dan 10 tagacha rasm tanlang");
        var order = (await images.QueryNoTracking().MaxAsync(g => (int?)g.SortOrder, ct) ?? 0) + 1;
        var added = new List<GalleryImage>();
        try
        {
            foreach (var f in uploads)
            {
                var img = new GalleryImage {Url = await files.SaveImageAsync(f, ct), SortOrder = order++};
                added.Add(img);
                images.Add(img);
            }
        }
        catch (BadRequestException)
        {
            // Bittasi yaroqsiz bo'lsa — oldin saqlangan fayllar ham o'chiriladi, hech biri qo'shilmaydi
            foreach (var a in added)
            {
                files.DeleteImage(a.Url);
                images.Remove(a);
            }
            throw;
        }
        await images.SaveChangesAsync(ct);
        return added.Select(g => new GalleryAdminDto(g.Id, g.Url, g.Caption, g.SortOrder)).ToList();
    }

    public async Task UpdateAsync(int id, GalleryUpdate input, CancellationToken ct)
    {
        var g = await images.GetByIdAsync(id, ct) ?? throw new NotFoundException("Rasm topilmadi");
        g.Caption = input.Caption.Clean(200);
        g.SortOrder = input.SortOrder;
        await images.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var g = await images.GetByIdAsync(id, ct) ?? throw new NotFoundException("Rasm topilmadi");
        await images.DeleteAsync(g, ct);
        files.DeleteImage(g.Url);
    }
}
