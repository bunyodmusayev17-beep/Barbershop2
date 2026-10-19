namespace KnyazBarber.Api.Exceptions;

public class UnauthorizedException(string message, string? code = null)
    : AppException(StatusCodes.Status401Unauthorized, message, code);
