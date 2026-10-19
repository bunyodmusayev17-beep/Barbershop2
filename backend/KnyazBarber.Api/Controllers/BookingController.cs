using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using KnyazBarber.Api.Dtos.Booking;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Controllers;

/// <summary>Mijoz saytdan onlayn navbatga yoziladi.</summary>
[ApiController]
[Route("api/booking")]
public class BookingController(IBookingService booking) : ControllerBase
{
    [HttpGet("slots")]
    public Task<SlotsResponse> Slots([FromQuery, Required] int serviceId, [FromQuery, Required] DateOnly date,
        [FromQuery] int? barberId, CancellationToken ct) =>
        booking.GetSlotsAsync(serviceId, date, barberId, ct);

    [HttpPost]
    [EnableRateLimiting(RateLimits.Booking)]
    public async Task<IActionResult> Create(CreateBookingRequest req, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await booking.CreateAsync(req, ct));
}
