namespace KnyazBarber.Api.Dtos.Booking;

/// <summary>Bo'sh vaqt (mahalliy, "HH:mm") va shu vaqtda bo'sh ustalar.</summary>
public record SlotDto(string Time, List<int> BarberIds);
