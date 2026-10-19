namespace KnyazBarber.Api.Entities;

/// <summary>Bazada uch tilda saqlanadigan matn.</summary>
public class LocalizedText
{
    public string Uz { get; set; } = "";
    public string Ru { get; set; } = "";
    public string En { get; set; } = "";

    public LocalizedText() { }
    public LocalizedText(string uz, string ru, string en) => (Uz, Ru, En) = (uz, ru, en);
}
