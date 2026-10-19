using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnyazBarber.Api.Dtos.TimeOff;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Controllers;

/// <summary>Ishlamaydigan vaqtlar: usta — faqat o'zi uchun, admin — istalgan usta uchun.</summary>
[ApiController]
[Route("api/staff/timeoffs")]
[Authorize(Policy = Policies.Staff)]
public class TimeOffsController(ITimeOffService timeOffs) : ControllerBase
{
    [HttpGet]
    public Task<List<StaffTimeOffDto>> List([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] int? barberId, CancellationToken ct) =>
        timeOffs.ListAsync(from, to, barberId, User.OwnBarberId(), ct);

    [HttpPost]
    public async Task<ActionResult<StaffTimeOffResult>> Create(StaffTimeOffInput input, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await timeOffs.CreateAsync(input, User.OwnBarberId(), ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await timeOffs.DeleteAsync(id, User.OwnBarberId(), ct);
        return NoContent();
    }
}
