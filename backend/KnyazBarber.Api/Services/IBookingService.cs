using KnyazBarber.Api.Dtos.Appointment;
using KnyazBarber.Api.Dtos.Booking;

namespace KnyazBarber.Api.Services;

public interface IBookingService
{
    /// <summary>Tanlangan kun uchun bo'sh vaqtlar (mahalliy vaqt) va har bir vaqtda bo'sh ustalar.</summary>
    Task<SlotsResponse> GetSlotsAsync(int serviceId, DateOnly date, int? barberId, CancellationToken ct);

    /// <summary>Mijoz saytdan yoziladi: ish vaqti, bo'sh vaqt va telefon limiti tekshiriladi.</summary>
    Task<BookingConfirmation> CreateAsync(CreateBookingRequest req, CancellationToken ct);

    /// <summary>
    /// Xodim qo'lda qo'shadi. Ish vaqti, minimal oldindan yozilish va telefon limiti tekshirilmaydi —
    /// faqat ustma-ust tushmasligi (baza darajasida).
    /// </summary>
    Task<AppointmentDto> CreateByStaffAsync(StaffBookingRequest req, int? actorUserId, CancellationToken ct);
}
