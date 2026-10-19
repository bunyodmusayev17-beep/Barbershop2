using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Dtos.Appointment;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;
using KnyazBarber.Api.Services.Telegram;

namespace KnyazBarber.Api.Services;

public class AppointmentService(
    IBaseRepository<Appointment> appointments,
    IBookingService booking,
    BusinessClock clock,
    ITelegramNotifier notifier) : IAppointmentService
{
    public async Task<List<AppointmentDto>> ListAsync(AppointmentFilter filter, int? ownBarberId, CancellationToken ct)
    {
        var fromDate = filter.From ?? clock.Today;
        var toDate = filter.To ?? fromDate.AddDays(30);
        if (toDate < fromDate) (fromDate, toDate) = (toDate, fromDate);
        if (toDate.DayNumber - fromDate.DayNumber > 370) toDate = fromDate.AddDays(370);

        var (start, _) = clock.DayRangeUtc(fromDate);
        var (_, end) = clock.DayRangeUtc(toDate);

        var q = appointments.QueryNoTracking().Include(a => a.Barber).Include(a => a.Service)
            .Where(a => a.StartUtc >= start && a.StartUtc < end);

        if (ownBarberId is not null) q = q.Where(a => a.BarberId == ownBarberId);
        else if (filter.BarberId is not null) q = q.Where(a => a.BarberId == filter.BarberId);

        if (filter.Status is not null) q = q.Where(a => a.Status == filter.Status);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            var digits = new string(s.Where(char.IsDigit).ToArray());
            q = q.Where(a => EF.Functions.ILike(a.ClientName, $"%{s}%") || (digits.Length >= 3 && a.ClientPhone.Contains(digits)));
        }

        var list = await q.OrderBy(a => a.StartUtc).Take(1000).ToListAsync(ct);
        return list.Select(a => a.ToDto(clock)).ToList();
    }

    public async Task<AppointmentDto> SetStatusAsync(int id, StatusUpdate body, int? ownBarberId, int? actorUserId, CancellationToken ct)
    {
        var a = await appointments.GetByIdAsync(id, ct, x => x.Barber, x => x.Service) ?? throw new NotFoundException("Navbat topilmadi");
        if (ownBarberId is not null && a.BarberId != ownBarberId) throw new ForbiddenException();

        var cancelled = a.Status != AppointmentStatus.Cancelled && body.Status == AppointmentStatus.Cancelled;
        a.Status = body.Status;
        try
        {
            await appointments.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsExclusionViolation())
        {
            // Bekor qilingan navbatni qayta tiklashda vaqt allaqachon band bo'lishi mumkin
            throw new ConflictException("Bu vaqt allaqachon band", BookingErrors.SlotTaken);
        }
        if (cancelled) notifier.Publish(new BotEvent(BotEventKind.Cancelled, a.Id, actorUserId));
        return a.ToDto(clock);
    }

    public Task<AppointmentDto> CreateAsync(StaffBookingRequest req, int? ownBarberId, int? actorUserId, CancellationToken ct)
    {
        if (ownBarberId is not null && req.BarberId != ownBarberId) throw new ForbiddenException();
        return booking.CreateByStaffAsync(req, actorUserId, ct);
    }
}
