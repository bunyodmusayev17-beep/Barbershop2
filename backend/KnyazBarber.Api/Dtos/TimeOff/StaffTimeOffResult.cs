using KnyazBarber.Api.Dtos.Appointment;

namespace KnyazBarber.Api.Dtos.TimeOff;

/// <summary>Yaratilgan ishlamaydigan vaqt + shu vaqtga to'g'ri kelib qolgan mavjud navbatlar (mijozlarga xabar berish uchun).</summary>
public record StaffTimeOffResult(StaffTimeOffDto TimeOff, List<AppointmentDto> Conflicts);
