using System.ComponentModel.DataAnnotations;

namespace KnyazBarber.Api.Dtos.TimeOff;

/// <summary>From/To — mahalliy vaqt. BarberId — faqat admin uchun (usta har doim o'zi uchun).</summary>
public record StaffTimeOffInput(int? BarberId, [Required] DateTime From, [Required] DateTime To, [MaxLength(200)] string? Reason);
