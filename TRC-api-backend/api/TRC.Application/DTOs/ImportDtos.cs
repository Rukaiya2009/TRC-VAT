using TRC.Domain.Enums;

namespace TRC.Application.DTOs;

public record CreateImportRequest(
    string Consignee,
    string HSCode,
    string Description,
    decimal InvoiceValueUsd,
    decimal ExchangeRate,
    decimal OtherCosts,
    DateTime? Date,
    bool HasCommercialInvoice = true,
    bool HasBillOfLading = true,
    bool HasMushak = true);

public record ImportDto(
    Guid Id,
    string Consignee,
    string HSCode,
    string Description,
    decimal InvoiceValueUsd,
    decimal ExchangeRate,
    decimal CfrValue,
    decimal OtherCosts,
    decimal AssessableValue,
    decimal TotalTax,
    ImportStatus Status,
    DateTime Date,
    IReadOnlyList<TaxLineDto> TaxBreakdown);
