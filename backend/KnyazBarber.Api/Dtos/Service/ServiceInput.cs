using System.ComponentModel.DataAnnotations;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Service;

public record ServiceInput(
    [Required] LocalizedText Name,
    LocalizedText? Description,
    [Range(0, 100_000_000)] decimal Price,
    [Range(5, 600)] int DurationMinutes,
    int SortOrder,
    bool IsActive,
    List<int>? BarberIds);
