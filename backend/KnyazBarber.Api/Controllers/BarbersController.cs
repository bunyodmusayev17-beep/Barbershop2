using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnyazBarber.Api.Dtos.Barber;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Controllers;

[ApiController]
[Route("api/admin/barbers")]
[Authorize(Policy = Policies.Admin)]
public class BarbersController(IBarberManagementService barbers) : ControllerBase
{
    [HttpGet]
    public Task<List<BarberAdminDto>> List(CancellationToken ct) => barbers.ListAsync(ct);

    [HttpPost]
    public async Task<ActionResult<BarberAdminDto>> Create(BarberInput input, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await barbers.CreateAsync(input, ct));

    [HttpPut("{id:int}")]
    public Task<BarberAdminDto> Update(int id, BarberInput input, CancellationToken ct) => barbers.UpdateAsync(id, input, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await barbers.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/photo")]
    [RequestSizeLimit(FileUploadService.MaxBytes + 64 * 1024)]
    public Task<BarberAdminDto> UploadPhoto(int id, IFormFile file, CancellationToken ct) => barbers.UploadPhotoAsync(id, file, ct);

    [HttpDelete("{id:int}/photo")]
    public Task<BarberAdminDto> DeletePhoto(int id, CancellationToken ct) => barbers.DeletePhotoAsync(id, ct);

    [HttpPost("{id:int}/timeoffs")]
    public Task<BarberAdminDto> AddTimeOff(int id, TimeOffInput input, CancellationToken ct) => barbers.AddTimeOffAsync(id, input, ct);

    [HttpDelete("{id:int}/timeoffs/{timeOffId:int}")]
    public Task<BarberAdminDto> DeleteTimeOff(int id, int timeOffId, CancellationToken ct) => barbers.DeleteTimeOffAsync(id, timeOffId, ct);
}
