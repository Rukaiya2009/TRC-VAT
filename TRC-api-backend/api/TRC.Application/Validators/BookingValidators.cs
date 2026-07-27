using FluentValidation;
using TRC.Application.DTOs;

namespace TRC.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password needs an uppercase letter.")
            .Matches("[0-9]").WithMessage("Password needs a digit.");
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone number is required.");
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").Matches("[0-9]");
    }
}

public class BookAppointmentRequestValidator : AbstractValidator<BookAppointmentRequest>
{
    public BookAppointmentRequestValidator()
    {
        RuleFor(x => x.ConsultationDayId).NotEmpty();
        RuleFor(x => x.SlotIndex).GreaterThanOrEqualTo(0);
    }
}

public class UpdateMeetingLinkRequestValidator : AbstractValidator<UpdateMeetingLinkRequest>
{
    public UpdateMeetingLinkRequestValidator()
    {
        RuleFor(x => x.MeetingLink)
            .NotEmpty()
            .Must(u => Uri.TryCreate(u, UriKind.Absolute, out var uri)
                       && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Meeting link must be a valid http(s) URL.");
    }
}
