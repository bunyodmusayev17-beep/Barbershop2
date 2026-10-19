using System.ComponentModel.DataAnnotations;

namespace KnyazBarber.Api.Dtos.Booking;

public record CreateBookingRequest(
    [Required] int ServiceId,
    int? BarberId,
    [Required] DateOnly Date,
    [Required] TimeOnly Time,
    [Required, MaxLength(100)] string ClientName,
    [Required, MaxLength(32)] string ClientPhone,
    [MaxLength(500)] string? Comment,
    [MaxLength(2)] string? Language);
