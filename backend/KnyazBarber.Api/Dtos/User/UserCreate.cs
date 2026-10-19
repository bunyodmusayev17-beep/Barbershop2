using System.ComponentModel.DataAnnotations;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.User;

public record UserCreate(
    [Required, RegularExpression("^[a-zA-Z0-9_.-]{3,64}$")] string Login,
    [EmailAddress, MaxLength(256)] string? Email,
    [Required, MinLength(8), MaxLength(128)] string Password,
    [Required] UserRole Role,
    int? BarberId,
    [MaxLength(40)] string? Telegram = null);
