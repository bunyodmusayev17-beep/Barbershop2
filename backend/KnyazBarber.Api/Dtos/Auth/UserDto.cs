namespace KnyazBarber.Api.Dtos.Auth;

public record UserDto(int Id, string Login, string Role, int? BarberId, string? BarberName);
