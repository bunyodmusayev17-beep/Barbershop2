using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Service;

public record ServiceAdminDto(int Id, LocalizedText Name, LocalizedText Description, decimal Price, int DurationMinutes,
    int SortOrder, bool IsActive, List<int> BarberIds);
