namespace KnyazBarber.Api.Exceptions;

public class ForbiddenException(string message = "Ruxsat yo'q", string? code = null)
    : AppException(StatusCodes.Status403Forbidden, message, code);
