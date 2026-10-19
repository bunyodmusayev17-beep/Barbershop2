using System.ComponentModel.DataAnnotations;

namespace KnyazBarber.Api.Dtos.Appointment;

/// <summary>Xodim qo'lda qo'shadigan navbat (telefon orqali yoki to'g'ridan-to'g'ri kelgan mijoz).</summary>
public record StaffBookingRequest(
    [Required] int ServiceId,
    [Required] int BarberId,
    [Required] DateOnly Date,
    [Required] TimeOnly Time,
    [Required, MaxLength(100)] string ClientName,
    [Required, MaxLength(32)] string ClientPhone,
    [MaxLength(500)] string? Comment);
