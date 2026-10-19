using KnyazBarber.Api.Dtos.TimeOff;

namespace KnyazBarber.Api.Services;

/// <summary>
/// Ishlamaydigan vaqtlar (kasallik, shaxsiy ish, ta'til). Usta — faqat o'zi uchun, admin — istalgan usta uchun.
/// Bu vaqtlarga onlayn navbat darhol yopiladi (AvailabilityService TimeOff'larni hisobga oladi).
/// </summary>
public interface ITimeOffService
{
    Task<List<StaffTimeOffDto>> ListAsync(DateOnly? from, DateOnly? to, int? barberId, int? ownBarberId, CancellationToken ct);
    Task<StaffTimeOffResult> CreateAsync(StaffTimeOffInput input, int? ownBarberId, CancellationToken ct);
    Task DeleteAsync(int id, int? ownBarberId, CancellationToken ct);
}
