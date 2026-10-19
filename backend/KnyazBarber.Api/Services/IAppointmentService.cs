using KnyazBarber.Api.Dtos.Appointment;

namespace KnyazBarber.Api.Services;

/// <summary>
/// Navbatlarni boshqarish (panel). ownBarberId: admin uchun null (hammasi), usta uchun — uning BarberId'si
/// (faqat o'z navbatlarini ko'radi va o'zgartiradi).
/// </summary>
public interface IAppointmentService
{
    Task<List<AppointmentDto>> ListAsync(AppointmentFilter filter, int? ownBarberId, CancellationToken ct);
    Task<AppointmentDto> SetStatusAsync(int id, StatusUpdate body, int? ownBarberId, int? actorUserId, CancellationToken ct);
    Task<AppointmentDto> CreateAsync(StaffBookingRequest req, int? ownBarberId, int? actorUserId, CancellationToken ct);
}
