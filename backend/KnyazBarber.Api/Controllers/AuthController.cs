using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using KnyazBarber.Api.Dtos.Auth;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting(RateLimits.Login)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req, CancellationToken ct) =>
        await auth.LoginAsync(req, Ip(), ct);

    /// <summary>Mini App'dan parolsiz kirish. Oddiy mijoz bo'lsa — 204 (sayt ommaviy rejimda qoladi).</summary>
    [HttpPost("telegram")]
    [EnableRateLimiting(RateLimits.Login)]
    public async Task<ActionResult<LoginResponse>> TelegramLogin(TelegramLoginRequest req, CancellationToken ct)
    {
        var result = await auth.TelegramLoginAsync(req, Ip(), ct);
        return result is null ? NoContent() : result;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct) =>
        await auth.MeAsync(User.UserId() ?? throw new UnauthorizedException("Sessiya tugagan"), ct);

    private string Ip() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "?";
}
