using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnyazBarber.Api.Dtos.Settings;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Controllers;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Policy = Policies.Admin)]
public class SettingsController(ISettingsService settings) : ControllerBase
{
    [HttpGet]
    public Task<SettingsDto> Get(CancellationToken ct) => settings.GetAsync(ct);

    [HttpPut]
    public async Task<IActionResult> Update(SettingsDto input, CancellationToken ct)
    {
        await settings.UpdateAsync(input, ct);
        return NoContent();
    }
}
