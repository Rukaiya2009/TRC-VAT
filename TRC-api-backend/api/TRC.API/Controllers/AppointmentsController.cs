using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Shared.Common;

namespace TRC.API.Controllers;

// M11 — booking. Prospect endpoints are gated by the account JWT (Prospect role); identity
// (UserId) comes from the signed token, never from the request body.
[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointments;
    public AppointmentsController(IAppointmentService appointments) => _appointments = appointments;

    private Guid UserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub"), out var id)
            ? id
            : throw new UnauthorizedAccessException("Token is missing or malformed.");

    // ---------------------------------------------------------------- prospect (phone token)

    [Authorize(Roles = "Prospect")]
    [HttpPost]
    public async Task<IActionResult> Book(BookAppointmentRequest req, CancellationToken ct)
        => Ok(ApiResponse<AppointmentDto>.Ok(await _appointments.BookAsync(UserId, req, ct)));

    [Authorize(Roles = "Prospect")]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AppointmentDto>>.Ok(await _appointments.GetMineAsync(UserId, ct)));

    // Cancelling does NOT count as a miss — we'd rather people cancel than no-show.
    [Authorize(Roles = "Prospect")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _appointments.CancelAsync(UserId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Booking cancelled." }));
    }

    // Self-reschedule. Closes 6h before the slot (config: Booking:ProspectRescheduleLeadHours);
    // after that the prospect can only cancel.
    [Authorize(Roles = "Prospect")]
    [HttpPut("{id:guid}/reschedule")]
    public async Task<IActionResult> Reschedule(Guid id, RescheduleAppointmentRequest req, CancellationToken ct)
        => Ok(ApiResponse<AppointmentDto>.Ok(await _appointments.RescheduleAsync(UserId, id, req, ct)));

    // ---------------------------------------------------------------- admin

    // Staff reschedule on a client's behalf. Tighter 1h lead time, and allowed past the
    // same-day 12:00 booking cutoff (config: Booking:AdminRescheduleLeadHours).
    [Authorize(Roles = "Admin,Auditor")]
    [HttpPut("{id:guid}/admin-reschedule")]
    public async Task<IActionResult> AdminReschedule(Guid id, RescheduleAppointmentRequest req, CancellationToken ct)
        => Ok(ApiResponse<AppointmentDto>.Ok(await _appointments.RescheduleAsAdminAsync(id, req, ct)));

    // Marking Missed increments the phone's miss count; the 3rd one blocks the number.
    [Authorize(Roles = "Admin,Auditor")]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, UpdateAppointmentStatusRequest req, CancellationToken ct)
        => Ok(ApiResponse<AppointmentDto>.Ok(await _appointments.SetStatusAsync(id, req.Status, ct)));

    // Manual fallback for the meeting link (and the override path if TRC moves off Google Meet).
    [Authorize(Roles = "Admin,Auditor")]
    [HttpPut("{id:guid}/meeting-link")]
    public async Task<IActionResult> SetMeetingLink(Guid id, UpdateMeetingLinkRequest req, CancellationToken ct)
        => Ok(ApiResponse<AppointmentDto>.Ok(await _appointments.SetMeetingLinkAsync(id, req.MeetingLink, ct)));

    [Authorize(Roles = "Admin")]
    [HttpPost("unblock/{phone}")]
    public async Task<IActionResult> Unblock(string phone, CancellationToken ct)
    {
        await _appointments.UnblockPhoneAsync(phone, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Phone unblocked." }));
    }
}
