namespace KnyazBarber.Api.Exceptions;

/// <summary>
/// Kutilgan xatolar uchun asos: ExceptionHandlingMiddleware ularni HTTP holat kodi va
/// ProblemDetails javobiga aylantiradi. Code — frontend 3 tilda xabar chiqarishi uchun (ixtiyoriy).
/// </summary>
public abstract class AppException(int statusCode, string message, string? code = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string? Code { get; } = code;
}
