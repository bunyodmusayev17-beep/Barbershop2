using System.ComponentModel.DataAnnotations;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Barber;

public record BarberInput(
    [Required, MaxLength(100)] string Name,
    LocalizedText? Title,
    LocalizedText? Bio,
    int SortOrder,
    bool IsActive,
    List<int>? ServiceIds,
    List<HoursDto>? Hours);
