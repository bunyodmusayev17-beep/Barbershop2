namespace KnyazBarber.Api.Dtos.Barber;

public record HoursDto(DayOfWeek Day, string Start, string End, bool IsDayOff);
