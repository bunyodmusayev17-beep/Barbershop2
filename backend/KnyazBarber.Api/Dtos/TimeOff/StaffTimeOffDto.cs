namespace KnyazBarber.Api.Dtos.TimeOff;

public record StaffTimeOffDto(int Id, int BarberId, string BarberName, string From, string To, string? Reason);
