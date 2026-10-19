using Microsoft.AspNetCore.Mvc;
using KnyazBarber.Api.Dtos.Public;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController(IPublicSiteService site) : ControllerBase
{
    [HttpGet("site")]
    public Task<SiteResponse> Site(CancellationToken ct) => site.GetSiteAsync(ct);
}
