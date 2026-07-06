using TRC.Application.DTOs;
using TRC.Domain.Entities;

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
