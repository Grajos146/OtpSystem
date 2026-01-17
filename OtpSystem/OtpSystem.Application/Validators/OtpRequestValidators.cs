using FluentValidation;
using OtpSystem.Application.DTO;

namespace OtpSystem.Application.Validators;

public class GenerateOtpRequestValidator : AbstractValidator<GenerateOtpRequest>
{
    public GenerateOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Enter a valid email address");
    }
}

public class ValidateOtpRequestValidator : AbstractValidator<ValidateOtpRequest>
{
    public ValidateOtpRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.CorrelationId).NotEmpty().WithMessage("CorrelationId is required");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("OTP Code is required")
            .Length(6)
            .WithMessage("OTP must be 6 digits")
            .Matches("^[0-9]+$")
            .WithMessage("OTP must only contain digits");
    }
}
