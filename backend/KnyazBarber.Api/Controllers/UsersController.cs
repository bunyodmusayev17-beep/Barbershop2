using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnyazBarber.Api.Dtos.User;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = Policies.Admin)]
public class UsersController(IUserService users) : ControllerBase
{
    [HttpGet]
    public Task<List<UserAdminDto>> List(CancellationToken ct) => users.ListAsync(ct);

    [HttpPost]
    public async Task<ActionResult<UserAdminDto>> Create(UserCreate input, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await users.CreateAsync(input, ct));

    [HttpPut("{id:int}")]
    public Task<UserAdminDto> Update(int id, UserUpdate input, CancellationToken ct) => users.UpdateAsync(id, input, User.UserId(), ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await users.DeleteAsync(id, User.UserId(), ct);
        return NoContent();
    }
}
