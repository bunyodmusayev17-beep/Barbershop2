using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnyazBarber.Api.Dtos.Review;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Controllers;

[ApiController]
[Route("api/admin/reviews")]
[Authorize(Policy = Policies.Admin)]
public class ReviewsController(IReviewService reviews) : ControllerBase
{
    [HttpGet]
    public Task<List<ReviewAdminDto>> List(CancellationToken ct) => reviews.ListAsync(ct);

    [HttpPost]
    public async Task<ActionResult<ReviewAdminDto>> Create(ReviewInput input, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await reviews.CreateAsync(input, ct));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ReviewInput input, CancellationToken ct)
    {
        await reviews.UpdateAsync(id, input, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await reviews.DeleteAsync(id, ct);
        return NoContent();
    }
}
