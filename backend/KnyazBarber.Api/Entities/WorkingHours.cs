namespace KnyazBarber.Api.Entities;

/// <summary>Ustaning haftalik ish jadvali (mahalliy vaqt, Asia/Tashkent).</summary>
public class WorkingHours
{
    public int Id { get; set; }
    public int BarberId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
    public bool IsDayOff { get; set; }
}
