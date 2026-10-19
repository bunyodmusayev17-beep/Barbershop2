namespace KnyazBarber.Api.Dtos.Booking;

public record SlotsResponse(DateOnly Date, List<SlotDto> Slots);
