namespace KnyazBarber.Api.Dtos.Booking;

/// <summary>Bitta ustaning bir kundagi bo'sh boshlanish vaqtlari (UTC).</summary>
public record BarberSlots(int BarberId, List<DateTimeOffset> StartsUtc);
