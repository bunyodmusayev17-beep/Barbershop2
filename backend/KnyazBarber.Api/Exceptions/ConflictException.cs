namespace KnyazBarber.Api.Exceptions;

public class ConflictException(string message, string? code = null)
    : AppException(StatusCodes.Status409Conflict, message, code);
