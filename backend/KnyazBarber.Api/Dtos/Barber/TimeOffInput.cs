using System.ComponentModel.DataAnnotations;

namespace KnyazBarber.Api.Dtos.Barber;

/// <summary>From/To — mahalliy vaqt, "yyyy-MM-ddTHH:mm".</summary>
public record TimeOffInput([Required] DateTime From, [Required] DateTime To, [MaxLength(200)] string? Reason);
