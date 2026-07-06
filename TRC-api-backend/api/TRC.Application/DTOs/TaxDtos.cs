using TRC.Domain.Enums;

namespace TRC.Application.DTOs;

public record TaxLineDto(TaxType TaxType, decimal BaseAmount, decimal Rate, decimal Amount);

public record TaxCalculationResult(
    decimal AssessableValue,
    decimal TotalTax,
    IReadOnlyList<TaxLineDto> Lines);
