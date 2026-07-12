using FluentValidation;
using TRC.Application.DTOs;

namespace TRC.Application.Validators;

public class SendOtpRequestValidator : AbstractValidator<SendOtpRequest>
{
    public SendOtpRequestValidator()
    {
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone number is required.");
    }
}

public class VerifyOtpRequestValidator : AbstractValidator<VerifyOtpRequest>
{
    public VerifyOtpRequestValidator()
    {
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Matches(@"^\d{4,8}$").WithMessage("Enter the numeric code you received.");
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
