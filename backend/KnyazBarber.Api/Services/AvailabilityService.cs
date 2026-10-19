using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Dtos.Booking;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;

namespace KnyazBarber.Api.Services;

/// <summary>Bazadan ma'lumot olib, har bir usta uchun bo'sh vaqtlarni hisoblaydi (hisobning o'zi — SlotCalculator).</summary>
public class AvailabilityService(
    IBaseRepository<SiteSettings> settings,
    IBaseRepository<Service> services,
    IBaseRepository<BarberService> barberServices,
    IBaseRepository<WorkingHours> workingHours,
    IBaseRepository<Appointment> appointments,
    IBaseRepository<TimeOff> timeOffs,
    BusinessClock clock) : IAvailabilityService
{
    public async Task<List<BarberSlots>> GetAsync(int serviceId, DateOnly date, int? barberId, CancellationToken ct)
    {
        var site = await settings.QueryNoTracking().OrderBy(x => x.Id).FirstAsync(ct);
        if (date < clock.Today || date > clock.Today.AddDays(site.BookingHorizonDays)) return [];

        var service = await services.QueryNoTracking().FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive, ct);
        if (service is null) return [];

        var barberIds = await barberServices.QueryNoTracking()
            .Where(bs => bs.ServiceId == serviceId && bs.Barber.IsActive && (barberId == null || bs.BarberId == barberId))
            .OrderBy(bs => bs.Barber.SortOrder)
            .Select(bs => bs.BarberId)
            .ToListAsync(ct);
        if (barberIds.Count == 0) return [];

        var (dayStart, dayEnd) = clock.DayRangeUtc(date);
        var dow = date.DayOfWeek;

        var hours = await workingHours.QueryNoTracking()
            .Where(w => barberIds.Contains(w.BarberId) && w.DayOfWeek == dow)
            .ToDictionaryAsync(w => w.BarberId, ct);

        var booked = await appointments.QueryNoTracking()
            .Where(a => barberIds.Contains(a.BarberId) && a.Status != AppointmentStatus.Cancelled
                        && a.StartUtc < dayEnd && a.EndUtc > dayStart)
            .Select(a => new {a.BarberId, a.StartUtc, a.EndUtc})
            .ToListAsync(ct);

        var off = await timeOffs.QueryNoTracking()
            .Where(t => barberIds.Contains(t.BarberId) && t.FromUtc < dayEnd && t.ToUtc > dayStart)
            .Select(t => new {t.BarberId, t.FromUtc, t.ToUtc})
            .ToListAsync(ct);

        var earliest = clock.UtcNow.AddMinutes(site.MinLeadMinutes);
        var result = new List<BarberSlots>();
        foreach (var id in barberIds)
        {
            WorkWindow? window = hours.TryGetValue(id, out var h) && !h.IsDayOff ? new WorkWindow(h.Start, h.End) : null;
            var busy = booked.Where(a => a.BarberId == id).Select(a => new BusyRange(a.StartUtc, a.EndUtc))
                .Concat(off.Where(t => t.BarberId == id).Select(t => new BusyRange(t.FromUtc, t.ToUtc)))
                .ToList();

            var starts = SlotCalculator.FreeStarts(new SlotQuery(
                date, window, busy, service.DurationMinutes, site.SlotStepMinutes, earliest, clock.Zone));
            result.Add(new BarberSlots(id, starts));
        }
        return result;
    }
}
