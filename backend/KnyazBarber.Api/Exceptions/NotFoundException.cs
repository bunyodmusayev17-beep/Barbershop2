namespace KnyazBarber.Api.Exceptions;

public class NotFoundException(string message = "Topilmadi", string? code = null)
    : AppException(StatusCodes.Status404NotFound, message, code);
