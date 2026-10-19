using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Dtos.Stats;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;

namespace KnyazBarber.Api.Services;

public class StatsService(IBaseRepository<Appointment> appointments, IBaseRepository<Barber> barbers, BusinessClock clock) : IStatsService
{
    /// <summary>Daromadga kiradigan holatlar: bekor qilinmagan va "kelmadi" emas.</summary>
    private static readonly AppointmentStatus[] Billable = [AppointmentStatus.Pending, AppointmentStatus.Confirmed, AppointmentStatus.Completed];

    public async Task<StatsDto> GetAsync(CancellationToken ct)
    {
        var today = clock.Today;
        var weekStartDate = today.AddDays(-6);
        var (weekStart, _) = clock.DayRangeUtc(weekStartDate);
        var (todayStart, todayEnd) = clock.DayRangeUtc(today);

        var week = await appointments.QueryNoTracking()
            .Where(a => a.StartUtc >= weekStart && a.StartUtc < todayEnd)
            .Select(a => new {a.BarberId, a.StartUtc, a.Status, a.Price})
            .ToListAsync(ct);

        var billable = week.Where(a => Billable.Contains(a.Status)).ToList();
        var todayList = billable.Where(a => a.StartUtc >= todayStart).ToList();

        var days = Enumerable.Range(0, 7).Select(i =>
        {
            var d = weekStartDate.AddDays(i);
            var (s, e) = clock.DayRangeUtc(d);
            var items = billable.Where(a => a.StartUtc >= s && a.StartUtc < e).ToList();
            return new DayStat(d.ToString("yyyy-MM-dd"), items.Count, items.Sum(a => a.Price));
        }).ToList();

        var active = await barbers.QueryNoTracking().Where(b => b.IsActive).OrderBy(b => b.SortOrder).ToListAsync(ct);
        var perBarber = active.Select(b => new BarberStat(b.Id, b.Name,
            todayList.Count(a => a.BarberId == b.Id),
            billable.Count(a => a.BarberId == b.Id),
            billable.Where(a => a.BarberId == b.Id).Sum(a => a.Price))).ToList();

        var now = clock.UtcNow;
        var upcomingQuery = appointments.QueryNoTracking()
            .Where(a => a.StartUtc >= now && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed));
        var upcomingCount = await upcomingQuery.CountAsync(ct);
        var nextUp = await upcomingQuery.Include(a => a.Barber).Include(a => a.Service)
            .OrderBy(a => a.StartUtc).Take(8).ToListAsync(ct);

        return new StatsDto(
            todayList.Count, todayList.Sum(a => a.Price),
            billable.Count, billable.Sum(a => a.Price),
            upcomingCount, week.Count(a => a.Status == AppointmentStatus.NoShow),
            days, perBarber, nextUp.Select(a => a.ToDto(clock)).ToList());
    }
}
