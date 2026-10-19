using System.ComponentModel.DataAnnotations;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Settings;

public record SettingsDto(
    [Required, MaxLength(100)] string BusinessName,
    [Required] LocalizedText Address,
    [Required, MaxLength(32)] string Phone,
    [MaxLength(64)] string? Telegram,
    [MaxLength(64)] string? Instagram,
    [Range(-90, 90)] double Latitude,
    [Range(-180, 180)] double Longitude,
    [Range(0, 5)] double Rating,
    [Range(0, 1_000_000)] int RatingCount,
    [Range(5, 120)] int SlotStepMinutes,
    [Range(0, 1440)] int MinLeadMinutes,
    [Range(1, 90)] int BookingHorizonDays);
