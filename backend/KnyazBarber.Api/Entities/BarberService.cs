namespace KnyazBarber.Api.Entities;

/// <summary>Qaysi usta qaysi xizmatni bajaradi.</summary>
public class BarberService
{
    public int BarberId { get; set; }
    public Barber Barber { get; set; } = null!;
    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;
}
