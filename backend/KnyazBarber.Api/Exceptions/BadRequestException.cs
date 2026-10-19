namespace KnyazBarber.Api.Exceptions;

public class BadRequestException(string message, string? code = null)
    : AppException(StatusCodes.Status400BadRequest, message, code);
