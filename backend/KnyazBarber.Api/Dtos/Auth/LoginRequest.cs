using System.ComponentModel.DataAnnotations;

namespace KnyazBarber.Api.Dtos.Auth;

public record LoginRequest([Required, MaxLength(64)] string Login, [Required, MaxLength(128)] string Password);
