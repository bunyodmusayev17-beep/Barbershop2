using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Barber;

public record BarberAdminDto(int Id, string Name, LocalizedText Title, LocalizedText Bio, string? PhotoUrl, int SortOrder,
    bool IsActive, List<int> ServiceIds, List<HoursDto> Hours, List<TimeOffDto> TimeOffs);
