using System.ComponentModel.DataAnnotations;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.User;

public record UserUpdate(
    [EmailAddress, MaxLength(256)] string? Email,
    [Required] UserRole Role,
    int? BarberId,
    bool IsActive,
    [MinLength(8), MaxLength(128)] string? NewPassword,
    [MaxLength(40)] string? Telegram = null);
