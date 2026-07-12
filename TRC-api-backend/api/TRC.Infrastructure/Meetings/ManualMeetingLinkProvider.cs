using Microsoft.Extensions.Options;
using TRC.Application.Interfaces;
using TRC.Application.Options;

namespace TRC.Infrastructure.Meetings;

// Fallback provider (client-approved "Option 2"). Returns the configured standing link, or null
// so an admin can paste one per booking via PUT /api/appointments/{id}/meeting-link.
//
// TO AUTOMATE LATER: add GoogleCalendarMeetingLinkProvider implementing this same interface —
// create a Calendar event with conferenceData and return the generated Meet URL — then swap the
// registration in DependencyInjection. No call-site changes anywhere. Requires TRC to supply a
// Workspace service account JSON key with domain-wide delegation + the consultant's calendar id.
public class ManualMeetingLinkProvider : IMeetingLinkProvider
{
    private readonly BookingOptions _opts;
    public ManualMeetingLinkProvider(IOptions<BookingOptions> opts) => _opts = opts.Value;

    public Task<string?> CreateAsync(DateTime startUtc, DateTime endUtc, string subject, CancellationToken ct = default) =>
        Task.FromResult(string.IsNullOrWhiteSpace(_opts.DefaultMeetingLink) ? null : _opts.DefaultMeetingLink);
}
