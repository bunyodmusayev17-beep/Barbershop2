using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnyazBarber.Api.Dtos.Gallery;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Controllers;

[ApiController]
[Route("api/admin/gallery")]
[Authorize(Policy = Policies.Admin)]
public class GalleryController(IGalleryService gallery) : ControllerBase
{
    [HttpGet]
    public Task<List<GalleryAdminDto>> List(CancellationToken ct) => gallery.ListAsync(ct);

    [HttpPost]
    [RequestSizeLimit(10 * (FileUploadService.MaxBytes + 64 * 1024))]
    public Task<List<GalleryAdminDto>> Upload([FromForm] List<IFormFile> files, CancellationToken ct) => gallery.UploadAsync(files, ct);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, GalleryUpdate input, CancellationToken ct)
    {
        await gallery.UpdateAsync(id, input, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await gallery.DeleteAsync(id, ct);
        return NoContent();
    }
}
