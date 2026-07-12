using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Shared.Common;

namespace TRC.API.Controllers;

// M11 — booking. Prospect endpoints are gated by the phone token issued at OTP verify:
// identity comes from the signed token, never from a phone number in the request body.
// (Trusting a body/query phone would let anyone cancel a stranger's booking.)
[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    public const string PhonePolicy = "PhoneVerified";

    private readonly IAppointmentService _appointments;
    public AppointmentsController(IAppointmentService appointments) => _appointments = appointments;

    private Guid PhoneProfileId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub"), out var id)
            ? id
            : throw new UnauthorizedAccessException("Phone token is missing or malformed.");

    // ---------------------------------------------------------------- prospect (phone token)

    [Authorize(Policy = PhonePolicy)]
    [HttpPost]
    public async Task<IActionResult> Book(BookAppointmentRequest req, CancellationToken ct)
        => Ok(ApiResponse<AppointmentDto>.Ok(await _appointments.BookAsync(PhoneProfileId, req, ct)));

    [Authorize(Policy = PhonePolicy)]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AppointmentDto>>.Ok(await _appointments.GetMineAsync(PhoneProfileId, ct)));

    // Cancelling does NOT count as a miss — we'd rather people cancel than no-show.
    [Authorize(Policy = PhonePolicy)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _appointments.CancelAsync(PhoneProfileId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Booking cancelled." }));
    }

    // ---------------------------------------------------------------- admin

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
