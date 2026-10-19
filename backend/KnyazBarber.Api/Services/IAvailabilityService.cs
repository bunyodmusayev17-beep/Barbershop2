using KnyazBarber.Api.Dtos.Booking;

namespace KnyazBarber.Api.Services;

public interface IAvailabilityService
{
    /// <summary>Har bir usta uchun bo'sh boshlanish vaqtlari. barberId null — xizmatni bajaradigan barcha faol ustalar.</summary>
    Task<List<BarberSlots>> GetAsync(int serviceId, DateOnly date, int? barberId, CancellationToken ct);
}
