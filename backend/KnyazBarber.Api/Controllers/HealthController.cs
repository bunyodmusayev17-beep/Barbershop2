using Microsoft.AspNetCore.Mvc;
using KnyazBarber.Api.Data;

namespace KnyazBarber.Api.Controllers;

/// <summary>Server va baza ishlayaptimi — monitoring va deploy tekshiruvi uchun.</summary>
[ApiController]
[Route("api/health")]
public class HealthController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var dbOk = await db.Database.CanConnectAsync(ct);
        return Ok(new {status = "ok", database = dbOk ? "connected" : "unavailable", time = DateTimeOffset.UtcNow});
    }
}
