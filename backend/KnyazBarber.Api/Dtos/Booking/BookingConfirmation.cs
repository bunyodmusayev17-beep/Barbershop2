namespace KnyazBarber.Api.Dtos.Booking;

public record BookingConfirmation(
    int Id,
    int BarberId,
    string BarberName,
    int ServiceId,
    DateOnly Date,
    string Time,
    string EndTime,
    decimal Price,
    string ClientName,
    string ClientPhone);
