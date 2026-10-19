namespace KnyazBarber.Api.Entities;

/// <summary>Ta'til, kasallik va boshqa ishlamaydigan vaqtlar.</summary>
public class TimeOff
{
    public int Id { get; set; }
    public int BarberId { get; set; }
    public DateTimeOffset FromUtc { get; set; }
    public DateTimeOffset ToUtc { get; set; }
    public string? Reason { get; set; }
}
