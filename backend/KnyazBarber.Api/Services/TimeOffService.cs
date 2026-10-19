using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Dtos.TimeOff;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;

namespace KnyazBarber.Api.Services;

public class TimeOffService(
    IBaseRepository<TimeOff> timeOffs,
    IBaseRepository<Barber> barbers,
    IBaseRepository<Appointment> appointments,
    BusinessClock clock) : ITimeOffService
{
    private const int MaxDays = 60;

    public async Task<List<StaffTimeOffDto>> ListAsync(DateOnly? from, DateOnly? to, int? barberId, int? ownBarberId, CancellationToken ct)
    {
        var (start, _) = clock.DayRangeUtc(from ?? clock.Today);
        var (_, end) = clock.DayRangeUtc(to ?? (from ?? clock.Today).AddDays(90));

        var q = timeOffs.QueryNoTracking().Where(t => t.ToUtc > start && t.FromUtc < end);
        if (ownBarberId is not null) q = q.Where(t => t.BarberId == ownBarberId);
        else if (barberId is not null) q = q.Where(t => t.BarberId == barberId);

        var items = await q.OrderBy(t => t.FromUtc).ToListAsync(ct);
        var names = await barbers.QueryNoTracking().ToDictionaryAsync(b => b.Id, b => b.Name, ct);
        return items.Select(t => ToDto(t, names.GetValueOrDefault(t.BarberId, ""))).ToList();
    }

    public async Task<StaffTimeOffResult> CreateAsync(StaffTimeOffInput input, int? ownBarberId, CancellationToken ct)
    {
        var barberId = ownBarberId ?? input.BarberId ?? throw new BadRequestException("Ustani tanlang");
        var barber = await barbers.GetByIdAsync(barberId, ct) ?? throw new NotFoundException("Usta topilmadi");

        if (input.To <= input.From) throw new BadRequestException("Tugash vaqti boshlanishdan keyin bo'lishi kerak");
        if ((input.To - input.From).TotalDays > MaxDays) throw new BadRequestException($"Bir martada {MaxDays} kundan oshmasin");

        var fromUtc = SlotCalculator.ToUtc(DateOnly.FromDateTime(input.From), TimeOnly.FromDateTime(input.From), clock.Zone);
        var toUtc = SlotCalculator.ToUtc(DateOnly.FromDateTime(input.To), TimeOnly.FromDateTime(input.To), clock.Zone);
        if (toUtc <= clock.UtcNow) throw new BadRequestException("O'tib ketgan vaqtni belgilab bo'lmaydi");

        var timeOff = await timeOffs.AddAsync(new TimeOff
        {
            BarberId = barber.Id,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Reason = string.IsNullOrWhiteSpace(input.Reason) ? null : input.Reason.Trim(),
        }, ct);

        // Shu oraliqqa tushib qolgan faol navbatlar — usta/admin mijozlarga xabar berishi kerak (avtomatik bekor qilinmaydi)
        var conflicts = await appointments.QueryNoTracking().Include(a => a.Barber).Include(a => a.Service)
            .Where(a => a.BarberId == barber.Id && a.StartUtc < toUtc && a.EndUtc > fromUtc
                        && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed))
            .OrderBy(a => a.StartUtc)
            .ToListAsync(ct);

        return new StaffTimeOffResult(ToDto(timeOff, barber.Name), conflicts.Select(a => a.ToDto(clock)).ToList());
    }

    public async Task DeleteAsync(int id, int? ownBarberId, CancellationToken ct)
    {
        var t = await timeOffs.GetByIdAsync(id, ct) ?? throw new NotFoundException();
        if (ownBarberId is not null && t.BarberId != ownBarberId) throw new ForbiddenException();
        await timeOffs.DeleteAsync(t, ct);
    }

    private StaffTimeOffDto ToDto(TimeOff t, string barberName) =>
        new(t.Id, t.BarberId, barberName, clock.ToLocal(t.FromUtc).ToString("yyyy-MM-ddTHH:mm"), clock.ToLocal(t.ToUtc).ToString("yyyy-MM-ddTHH:mm"), t.Reason);
}
