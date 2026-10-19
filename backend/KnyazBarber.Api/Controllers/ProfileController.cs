using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnyazBarber.Api.Dtos.Profile;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Controllers;

/// <summary>Usta kabineti → "Profilim": usta faqat o'z rasmi va ishlarini boshqaradi (barber_id token'dan olinadi).</summary>
[ApiController]
[Route("api/staff/profile")]
[Authorize(Policy = Policies.Staff)]
public class ProfileController(IBarberProfileService profile) : ControllerBase
{
    [HttpGet]
    public Task<BarberProfileDto> Get(CancellationToken ct) => profile.GetAsync(OwnId(), ct);

    [HttpPost("photo")]
    [RequestSizeLimit(FileUploadService.MaxBytes + 64 * 1024)]
    public Task<BarberProfileDto> UploadPhoto(IFormFile file, CancellationToken ct) => profile.UploadPhotoAsync(OwnId(), file, ct);

    [HttpDelete("photo")]
    public Task<BarberProfileDto> DeletePhoto(CancellationToken ct) => profile.DeletePhotoAsync(OwnId(), ct);

    [HttpPost("works")]
    [RequestSizeLimit(10 * (FileUploadService.MaxBytes + 64 * 1024))]
    public Task<BarberProfileDto> UploadWorks([FromForm] List<IFormFile> files, CancellationToken ct) => profile.UploadWorksAsync(OwnId(), files, ct);

    [HttpDelete("works/{id:int}")]
    public Task<BarberProfileDto> DeleteWork(int id, CancellationToken ct) => profile.DeleteWorkAsync(OwnId(), id, ct);

    private int OwnId() => User.BarberId() ?? throw new ForbiddenException("Bu bo'lim faqat ustalar uchun");
}
