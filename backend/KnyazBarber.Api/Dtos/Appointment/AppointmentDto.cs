using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Appointment;

public record AppointmentDto(
    int Id, int BarberId, string BarberName, int ServiceId, LocalizedText ServiceName,
    string ClientName, string ClientPhone, string Date, string Time, string EndTime,
    string Status, decimal Price, string? Comment, string Language, DateTimeOffset CreatedAt);
