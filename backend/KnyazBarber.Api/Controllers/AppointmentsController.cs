using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnyazBarber.Api.Dtos.Appointment;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Controllers;

/// <summary>Navbatlar: admin hammasini ko'radi, usta — faqat o'zinikini (barber_id claim bo'yicha majburiy filtr).</summary>
[ApiController]
[Route("api/admin/appointments")]
[Authorize(Policy = Policies.Staff)]
public class AppointmentsController(IAppointmentService appointments) : ControllerBase
{
    [HttpGet]
    public Task<List<AppointmentDto>> List([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] int? barberId,
        [FromQuery] AppointmentStatus? status, [FromQuery] string? search, CancellationToken ct) =>
        appointments.ListAsync(new AppointmentFilter(from, to, barberId, status, search), User.OwnBarberId(), ct);

    [HttpPatch("{id:int}/status")]
    public Task<AppointmentDto> SetStatus(int id, StatusUpdate body, CancellationToken ct) =>
        appointments.SetStatusAsync(id, body, User.OwnBarberId(), User.UserId(), ct);

    [HttpPost]
    public async Task<IActionResult> Create(StaffBookingRequest req, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await appointments.CreateAsync(req, User.OwnBarberId(), User.UserId(), ct));
}
