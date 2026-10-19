namespace KnyazBarber.Api.Entities;

public class Appointment
{
    public int Id { get; set; }
    public int BarberId { get; set; }
    public Barber Barber { get; set; } = null!;
    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public string ClientName { get; set; } = "";
    public string ClientPhone { get; set; } = "";
    public DateTimeOffset StartUtc { get; set; }
    public DateTimeOffset EndUtc { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    /// <summary>Navbat paytidagi narx — keyin xizmat narxi o'zgarsa ham saqlanib qoladi.</summary>
    public decimal Price { get; set; }
    public string? Comment { get; set; }
    public string Language { get; set; } = "uz";
    /// <summary>Ustaga "2 soat qoldi" eslatmasi yuborilgan vaqt (takror yuborilmasligi uchun).</summary>
    public DateTimeOffset? ReminderSentAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
