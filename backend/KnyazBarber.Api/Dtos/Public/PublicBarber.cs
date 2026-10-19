using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Public;

public record PublicBarber(int Id, string Name, LocalizedText Title, LocalizedText Bio, string? PhotoUrl, List<int> ServiceIds, List<DayOfWeek> DaysOff);
