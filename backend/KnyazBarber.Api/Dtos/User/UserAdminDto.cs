namespace KnyazBarber.Api.Dtos.User;

public record UserAdminDto(int Id, string Login, string? Email, string Role, int? BarberId, string? BarberName, bool IsActive, DateTimeOffset CreatedAt,
    string? Telegram, bool TelegramLinked);
