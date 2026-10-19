using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnyazBarber.Api.Dtos.Service;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Controllers;

[ApiController]
[Route("api/admin/services")]
[Authorize(Policy = Policies.Admin)]
public class ServicesController(IServiceCatalogService catalog) : ControllerBase
{
    [HttpGet]
    public Task<List<ServiceAdminDto>> List(CancellationToken ct) => catalog.ListAsync(ct);

    [HttpPost]
    public async Task<ActionResult<ServiceAdminDto>> Create(ServiceInput input, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await catalog.CreateAsync(input, ct));

    [HttpPut("{id:int}")]
    public Task<ServiceAdminDto> Update(int id, ServiceInput input, CancellationToken ct) => catalog.UpdateAsync(id, input, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await catalog.DeleteAsync(id, ct);
        return NoContent();
    }
}
