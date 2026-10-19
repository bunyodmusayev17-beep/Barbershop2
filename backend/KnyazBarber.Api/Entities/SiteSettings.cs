namespace KnyazBarber.Api.Entities;

/// <summary>Sayt sozlamalari — bitta qator (Id = 1).</summary>
public class SiteSettings
{
    public int Id { get; set; } = 1;
    public string BusinessName { get; set; } = "";
    public LocalizedText Address { get; set; } = new();
    public string Phone { get; set; } = "";
    public string? Telegram { get; set; }
    public string? Instagram { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Rating { get; set; }
    public int RatingCount { get; set; }
    /// <summary>Bo'sh vaqtlar qadami, daqiqa.</summary>
    public int SlotStepMinutes { get; set; } = 15;
    /// <summary>Hozirdan necha daqiqa keyingi vaqtga yozilish mumkin.</summary>
    public int MinLeadMinutes { get; set; } = 30;
    /// <summary>Necha kun oldinga yozilish mumkin.</summary>
    public int BookingHorizonDays { get; set; } = 14;
    /// <summary>Kechki hisobot oxirgi marta qaysi kun uchun yuborilgan (Telegram).</summary>
    public DateOnly? LastReportDate { get; set; }
}
