namespace KnyazBarber.Api.Entities;

public class GalleryImage
{
    public int Id { get; set; }
    public string Url { get; set; } = "";
    public LocalizedText Caption { get; set; } = new();
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
