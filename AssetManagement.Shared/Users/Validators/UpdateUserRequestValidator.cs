using AssetManagement.Shared.Users.Dtos;
using FluentValidation;

namespace AssetManagement.Shared.Users.Validators;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Validation.FieldRequired")
            .MaximumLength(120)
            .WithMessage("Validation.FieldMaxLength")
            .Matches(@"^[a-zA-ZÀ-ÿ ]+$")
            .WithMessage("Validation.Name")
            .Must(name => !name.Contains("  "))
            .WithMessage("Validation.Name");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Validation.FieldRequired")
            .EmailAddress()
            .WithMessage("Validation.EmailInvalid")
            .MaximumLength(200)
            .WithMessage("Validation.FieldMaxLength");
    }
}