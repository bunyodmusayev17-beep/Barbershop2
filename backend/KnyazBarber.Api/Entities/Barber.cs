namespace KnyazBarber.Api.Entities;

public class Barber
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public LocalizedText Title { get; set; } = new();
    public LocalizedText Bio { get; set; } = new();
    public string? PhotoUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public List<BarberService> Services { get; set; } = [];
    public List<WorkingHours> WorkingHours { get; set; } = [];
    public List<TimeOff> TimeOffs { get; set; } = [];
    /// <summary>Ustaning o'zi yuklagan ish rasmlari (galereyada ham ko'rinadi).</summary>
    public List<GalleryImage> Works { get; set; } = [];
}
