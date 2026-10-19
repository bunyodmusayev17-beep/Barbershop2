namespace KnyazBarber.Api.Dtos.Auth;

public record LoginResponse(string Token, DateTimeOffset ExpiresAt, UserDto User);
