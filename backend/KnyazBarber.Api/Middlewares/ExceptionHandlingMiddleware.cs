using Microsoft.AspNetCore.Mvc;
using KnyazBarber.Api.Exceptions;

namespace KnyazBarber.Api.Middlewares;

/// <summary>
/// Xatolarni bitta joyda ushlaydi. AppException — kutilgan xato (400/401/403/404/409), matni foydalanuvchiga ko'rsatiladi.
/// Boshqa hammasi — 500, tafsiloti faqat logga yoziladi. Javob — ProblemDetails (title, detail, status, code).
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Mijoz so'rovni o'zi uzdi — javob yuborishning hojati yo'q
        }
        catch (AppException ex) when (!context.Response.HasStarted)
        {
            await WriteAsync(context, ex.StatusCode, ex.Message, ex.Code);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            logger.LogError(ex, "Kutilmagan xato: {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "Serverda xatolik yuz berdi", null);
        }
    }

    private static Task WriteAsync(HttpContext context, int status, string message, string? code)
    {
        var problem = new ProblemDetails {Status = status, Title = message, Detail = message};
        if (code is not null) problem.Extensions["code"] = code;
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}
