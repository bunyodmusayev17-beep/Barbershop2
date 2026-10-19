using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnyazBarber.Api.Dtos.Stats;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Controllers;

[ApiController]
[Route("api/admin/stats")]
[Authorize(Policy = Policies.Admin)]
public class StatsController(IStatsService stats) : ControllerBase
{
    [HttpGet]
    public Task<StatsDto> Get(CancellationToken ct) => stats.GetAsync(ct);
}
