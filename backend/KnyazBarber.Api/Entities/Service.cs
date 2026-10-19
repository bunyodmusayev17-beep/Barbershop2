namespace KnyazBarber.Api.Entities;

/// <summary>Sartaroshxona xizmati (soch olish, soqol va h.k.).</summary>
public class Service
{
    public int Id { get; set; }
    public LocalizedText Name { get; set; } = new();
    public LocalizedText Description { get; set; } = new();
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public List<BarberService> Barbers { get; set; } = [];
}
