using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Public;

public record PublicSettings(
    string BusinessName, LocalizedText Address, string Phone, string? Telegram, string? Instagram,
    double Latitude, double Longitude, double Rating, int RatingCount, int BookingHorizonDays,
    List<PublicHours> Hours);
