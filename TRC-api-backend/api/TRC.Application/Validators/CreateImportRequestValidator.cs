using FluentValidation;
using TRC.Application.DTOs;

namespace TRC.Application.Validators;

// FR-3.4: HS code required, invoice value > 0, exchange rate > 0.
public class CreateImportRequestValidator : AbstractValidator<CreateImportRequest>
{
    public CreateImportRequestValidator()
    {
        RuleFor(x => x.Consignee).NotEmpty();
        RuleFor(x => x.HSCode).NotEmpty().MinimumLength(6)
            .WithMessage("HS code must be at least 6 digits.");
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.InvoiceValueUsd).GreaterThan(0);
        RuleFor(x => x.ExchangeRate).GreaterThan(0);
        RuleFor(x => x.OtherCosts).GreaterThanOrEqualTo(0);
    }
}
