namespace KnyazBarber.Api.Entities;

public class GalleryImage
{
    public int Id { get; set; }
    public string Url { get; set; } = "";
    public LocalizedText Caption { get; set; } = new();
    public int SortOrder { get; set; }
    /// <summary>Usta yuklagan ish bo'lsa — o'sha usta; admin yuklagan umumiy rasm — null.</summary>
    public int? BarberId { get; set; }
    public Barber? Barber { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
