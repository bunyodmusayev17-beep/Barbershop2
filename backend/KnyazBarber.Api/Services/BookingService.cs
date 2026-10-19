using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Dtos.Appointment;
using KnyazBarber.Api.Dtos.Booking;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;
using KnyazBarber.Api.Services.Telegram;

namespace KnyazBarber.Api.Services;

public class BookingService(
    IBaseRepository<Appointment> appointments,
    IBaseRepository<Service> services,
    IBaseRepository<Barber> barbers,
    IBaseRepository<BarberService> barberServices,
    IAvailabilityService availability,
    BusinessClock clock,
    ITelegramNotifier notifier,
    ILogger<BookingService> logger) : IBookingService
{
    /// <summary>Bitta telefon raqamdan bir vaqtda nechta faol (kelajakdagi) navbat bo'lishi mumkin.</summary>
    public const int MaxActivePerPhone = 3;

    public async Task<SlotsResponse> GetSlotsAsync(int serviceId, DateOnly date, int? barberId, CancellationToken ct)
    {
        var perBarber = await availability.GetAsync(serviceId, date, barberId, ct);
        var slots = perBarber
            .SelectMany(b => b.StartsUtc.Select(s => (Start: s, b.BarberId)))
            .GroupBy(x => x.Start)
            .OrderBy(g => g.Key)
            .Select(g => new SlotDto(clock.ToLocal(g.Key).ToString("HH:mm"), g.Select(x => x.BarberId).ToList()))
            .ToList();
        return new SlotsResponse(date, slots);
    }

    public async Task<BookingConfirmation> CreateAsync(CreateBookingRequest req, CancellationToken ct)
    {
        var name = CleanName(req.ClientName);
        var phone = CleanPhone(req.ClientPhone);

        var service = await services.Query().FirstOrDefaultAsync(s => s.Id == req.ServiceId && s.IsActive, ct)
            ?? throw new NotFoundException("Xizmat topilmadi", BookingErrors.ServiceNotFound);

        if (req.BarberId is { } requested && !await barberServices.QueryNoTracking().AnyAsync(
                bs => bs.BarberId == requested && bs.ServiceId == service.Id && bs.Barber.IsActive, ct))
            throw new NotFoundException("Usta topilmadi yoki bu xizmatni bajarmaydi", BookingErrors.BarberNotFound);

        var now = clock.UtcNow;
        var activeCount = await appointments.QueryNoTracking().CountAsync(a => a.ClientPhone == phone && a.StartUtc > now
            && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.NoShow, ct);
        if (activeCount >= MaxActivePerPhone)
            throw new ConflictException("Bu raqamdan juda ko'p faol navbat bor", BookingErrors.LimitReached);

        var startUtc = SlotCalculator.ToUtc(req.Date, req.Time, clock.Zone);
        var slots = await availability.GetAsync(service.Id, req.Date, req.BarberId, ct);
        var candidates = slots.Where(s => s.StartsUtc.Contains(startUtc)).Select(s => s.BarberId).ToList();
        if (candidates.Count == 0)
            throw new ConflictException("Bu vaqt bo'sh emas", BookingErrors.SlotUnavailable);

        var barberId = req.BarberId ?? await PickLeastBusyAsync(candidates, req.Date, ct);
        var comment = string.IsNullOrWhiteSpace(req.Comment) ? null : req.Comment.Trim();
        var language = req.Language ?? "uz";

        var appointment = new Appointment
        {
            BarberId = barberId,
            ServiceId = service.Id,
            ClientName = name,
            ClientPhone = phone,
            StartUtc = startUtc,
            EndUtc = startUtc.AddMinutes(service.DurationMinutes),
            Price = service.Price,
            Comment = comment?[..Math.Min(500, comment.Length)],
            Language = language is "ru" or "en" ? language : "uz",
            // Avtomatik tasdiq — admin/usta Telegram'da xabar oladi, kerak bo'lsa bekor qiladi
            Status = AppointmentStatus.Confirmed,
            CreatedAt = now,
        };

        try
        {
            await appointments.AddAsync(appointment, ct);
        }
        catch (DbUpdateException ex) when (ex.IsExclusionViolation())
        {
            // Tekshiruv va yozish orasida kimdir shu vaqtni olib qo'ydi — baza buni to'xtatdi.
            logger.LogInformation("Navbat to'qnashuvi: usta {BarberId}, {Start}", barberId, startUtc);
            throw new ConflictException("Bu vaqtni hozirgina boshqa mijoz band qildi", BookingErrors.SlotTaken);
        }

        var barber = await barbers.GetByIdAsync(barberId, ct);
        notifier.Publish(new BotEvent(BotEventKind.Created, appointment.Id));

        var start = clock.ToLocal(appointment.StartUtc);
        var end = clock.ToLocal(appointment.EndUtc);
        return new BookingConfirmation(
            appointment.Id, appointment.BarberId, barber?.Name ?? "", appointment.ServiceId, DateOnly.FromDateTime(start.DateTime),
            start.ToString("HH:mm"), end.ToString("HH:mm"), appointment.Price, appointment.ClientName, appointment.ClientPhone);
    }

    public async Task<AppointmentDto> CreateByStaffAsync(StaffBookingRequest req, int? actorUserId, CancellationToken ct)
    {
        var name = CleanName(req.ClientName);
        var phone = CleanPhone(req.ClientPhone);
        var service = await services.GetByIdAsync(req.ServiceId, ct)
            ?? throw new NotFoundException("Xizmat topilmadi", BookingErrors.ServiceNotFound);
        var barber = await barbers.GetByIdAsync(req.BarberId, ct)
            ?? throw new NotFoundException("Usta topilmadi", BookingErrors.BarberNotFound);

        var startUtc = SlotCalculator.ToUtc(req.Date, req.Time, clock.Zone);
        var appointment = new Appointment
        {
            BarberId = barber.Id,
            ServiceId = service.Id,
            ClientName = name,
            ClientPhone = phone,
            StartUtc = startUtc,
            EndUtc = startUtc.AddMinutes(service.DurationMinutes),
            Price = service.Price,
            Comment = string.IsNullOrWhiteSpace(req.Comment) ? null : req.Comment.Trim(),
            Status = AppointmentStatus.Confirmed,
            CreatedAt = clock.UtcNow,
        };
        try
        {
            await appointments.AddAsync(appointment, ct);
        }
        catch (DbUpdateException ex) when (ex.IsExclusionViolation())
        {
            appointments.Remove(appointment);
            throw new ConflictException("Bu vaqtda ustada boshqa navbat bor", BookingErrors.SlotTaken);
        }
        appointment.Barber = barber;
        appointment.Service = service;
        notifier.Publish(new BotEvent(BotEventKind.Created, appointment.Id, actorUserId));
        return appointment.ToDto(clock);
    }

    private static string CleanName(string clientName)
    {
        var name = clientName.Trim();
        if (name.Length is < 2 or > 100)
            throw new BadRequestException("Ism 2–100 belgi bo'lishi kerak", BookingErrors.NameInvalid);
        return name;
    }

    private static string CleanPhone(string clientPhone) =>
        PhoneNumber.Normalize(clientPhone) ?? throw new BadRequestException("Telefon raqam noto'g'ri", BookingErrors.PhoneInvalid);

    /// <summary>"Istalgan usta" tanlanganda — o'sha kuni eng kam navbati bor usta.</summary>
    private async Task<int> PickLeastBusyAsync(List<int> candidates, DateOnly date, CancellationToken ct)
    {
        var (dayStart, dayEnd) = clock.DayRangeUtc(date);
        var load = await appointments.QueryNoTracking()
            .Where(a => candidates.Contains(a.BarberId) && a.Status != AppointmentStatus.Cancelled
                        && a.StartUtc >= dayStart && a.StartUtc < dayEnd)
            .GroupBy(a => a.BarberId)
            .Select(g => new {BarberId = g.Key, Count = g.Count()})
            .ToDictionaryAsync(x => x.BarberId, x => x.Count, ct);

        // candidates allaqachon SortOrder bo'yicha tartiblangan — teng bo'lsa birinchisi tanlanadi
        return candidates.OrderBy(id => load.GetValueOrDefault(id)).First();
    }
}
