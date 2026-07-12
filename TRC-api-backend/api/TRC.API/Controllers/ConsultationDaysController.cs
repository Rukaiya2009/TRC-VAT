using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Shared.Common;

namespace TRC.API.Controllers;

[ApiController]
[Route("api/consultation-days")]
public class ConsultationDaysController : ControllerBase
{
    private readonly IAppointmentService _appointments;
    public ConsultationDaysController(IAppointmentService appointments) => _appointments = appointments;

    // PUBLIC on purpose — prospects should see availability before being asked for a phone number.
    // Rolling days are generated on read, so this is never empty and needs no cron job.
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Available(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ConsultationDayDto>>.Ok(await _appointments.GetAvailabilityAsync(ct)));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Publish(CreateConsultationDayRequest req, CancellationToken ct)
        => Ok(ApiResponse<ConsultationDayDto>.Ok(await _appointments.PublishDayAsync(req, ct)));

    // Close a date (holiday, Eid, leave).
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct)
        => Ok(ApiResponse<ConsultationDayDto>.Ok(await _appointments.CloseDayAsync(id, ct)));

    [Authorize(Roles = "Admin,Auditor")]
    [HttpGet("{id:guid}/appointments")]
    public async Task<IActionResult> Bookings(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AppointmentDto>>.Ok(await _appointments.GetForDayAsync(id, ct)));
}
