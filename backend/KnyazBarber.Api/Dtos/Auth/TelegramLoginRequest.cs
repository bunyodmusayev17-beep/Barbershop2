using System.ComponentModel.DataAnnotations;

namespace KnyazBarber.Api.Dtos.Auth;

public record TelegramLoginRequest([Required, MaxLength(4096)] string InitData);
