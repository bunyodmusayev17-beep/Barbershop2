using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Dtos.Public;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Repositories;

namespace KnyazBarber.Api.Services;

public class PublicSiteService(
    IBaseRepository<SiteSettings> settings,
    IBaseRepository<Barber> barbers,
    IBaseRepository<Service> services,
    IBaseRepository<Review> reviews,
    IBaseRepository<GalleryImage> gallery) : IPublicSiteService
{
    public async Task<SiteResponse> GetSiteAsync(CancellationToken ct)
    {
        var s = await settings.QueryNoTracking().OrderBy(x => x.Id).FirstAsync(ct);

        var activeBarbers = await barbers.QueryNoTracking().Where(b => b.IsActive).OrderBy(b => b.SortOrder)
            .Select(b => new
            {
                b.Id, b.Name, b.Title, b.Bio, b.PhotoUrl,
                ServiceIds = b.Services.Where(x => x.Service.IsActive).Select(x => x.ServiceId).ToList(),
                Hours = b.WorkingHours.ToList(),
            })
            .ToListAsync(ct);

        var activeServices = await services.QueryNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder)
            .Select(x => new PublicService(x.Id, x.Name, x.Description, x.Price, x.DurationMinutes,
                x.Barbers.Where(b => b.Barber.IsActive).Select(b => b.BarberId).ToList()))
            .ToListAsync(ct);

        // Umumiy ish vaqti: har kuni ishlaydigan ustalarning eng erta ochilishi va eng kech yopilishi
        var hours = Enum.GetValues<DayOfWeek>().Select(day =>
        {
            var open = activeBarbers.SelectMany(b => b.Hours).Where(h => h.DayOfWeek == day && !h.IsDayOff).ToList();
            return open.Count == 0
                ? new PublicHours(day, null, null)
                : new PublicHours(day, open.Min(h => h.Start).ToString("HH:mm"), open.Max(h => h.End).ToString("HH:mm"));
        }).ToList();

        var published = await reviews.QueryNoTracking().Where(r => r.IsPublished).OrderByDescending(r => r.CreatedAt)
            .Take(20).Select(r => new PublicReview(r.Id, r.AuthorName, r.Rating, r.Text, r.CreatedAt)).ToListAsync(ct);

        // Faol bo'lmagan ustaning ishlari saytda ko'rinmaydi
        var images = await gallery.QueryNoTracking().Where(g => g.BarberId == null || g.Barber!.IsActive)
            .OrderBy(g => g.SortOrder).ThenBy(g => g.Id)
            .Select(g => new PublicImage(g.Id, g.Url, g.Caption, g.BarberId)).ToListAsync(ct);

        return new SiteResponse(
            new PublicSettings(s.BusinessName, s.Address, s.Phone, s.Telegram, s.Instagram, s.Latitude, s.Longitude,
                s.Rating, s.RatingCount, s.BookingHorizonDays, hours),
            activeServices,
            activeBarbers.Select(b => new PublicBarber(b.Id, b.Name, b.Title, b.Bio, b.PhotoUrl, b.ServiceIds,
                b.Hours.Where(h => h.IsDayOff).Select(h => h.DayOfWeek).ToList())).ToList(),
            published,
            images);
    }
}
