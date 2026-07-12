using TRC.Application.DTOs;
using TRC.Domain.Entities;
using TRC.Domain.Enums;

namespace TRC.Application.Interfaces;

// Section 8 engine. Pure & deterministic — unit-tested against the Bill of Entry.
public interface ITaxCalculationService
{
    TaxCalculationResult Calculate(decimal invoiceValueUsd, decimal exchangeRate, decimal otherCosts);
}

// §8.2 payment-vs-BOE variance quick-check.
public interface IVarianceService
{
    QuickCheckResult Evaluate(decimal paymentValue, decimal boeValue);
}

// §9 configurable rule engine. Phase A rules live at launch.
public interface IRiskEngine
{
    Task<RiskAssessmentResult> AssessAsync(Import import, CancellationToken ct = default);
}

public interface IImportService
{
    Task<ImportDto> CreateAsync(CreateImportRequest request, Guid? userId, CancellationToken ct = default);
    Task<ImportDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ImportDto>> ListAsync(CancellationToken ct = default);
    Task<RiskAssessmentResult?> AssessAsync(Guid importId, Guid? assessedBy, CancellationToken ct = default);
}

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResult?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResult?> RefreshAsync(string refreshToken, CancellationToken ct = default);
}

// M9 — phone verification. Gates the booking funnel (FR-9.1/9.2).
public interface IOtpService
{
    Task<SendOtpResult> SendAsync(SendOtpRequest request, CancellationToken ct = default);
    Task<VerifyOtpResult> VerifyAsync(VerifyOtpRequest request, CancellationToken ct = default);
}

// M11 — consultation days + appointment booking.
public interface IAppointmentService
{
    // Public: availability is visible without OTP (less friction before committing).
    Task<IReadOnlyList<ConsultationDayDto>> GetAvailabilityAsync(CancellationToken ct = default);

    // Prospect (requires a verified phone token).
    Task<AppointmentDto> BookAsync(Guid phoneProfileId, BookAppointmentRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<AppointmentDto>> GetMineAsync(Guid phoneProfileId, CancellationToken ct = default);
    Task CancelAsync(Guid phoneProfileId, Guid appointmentId, CancellationToken ct = default);

    // Admin.
    Task<ConsultationDayDto> PublishDayAsync(CreateConsultationDayRequest request, CancellationToken ct = default);
    Task<ConsultationDayDto> CloseDayAsync(Guid consultationDayId, CancellationToken ct = default);
    Task<IReadOnlyList<AppointmentDto>> GetForDayAsync(Guid consultationDayId, CancellationToken ct = default);
    Task<AppointmentDto> SetStatusAsync(Guid appointmentId, AppointmentStatus status, CancellationToken ct = default);
    Task<AppointmentDto> SetMeetingLinkAsync(Guid appointmentId, string meetingLink, CancellationToken ct = default);
    Task UnblockPhoneAsync(string phone, CancellationToken ct = default);
}
