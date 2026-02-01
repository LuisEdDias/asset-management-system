using AssetManagement.Application.Users.Dtos;
using FluentValidation;

namespace AssetManagement.Application.Users.Validators;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Validation.FieldRequired")
            .MaximumLength(120)
            .WithMessage("Validation.FieldMaxLength");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Validation.FieldRequired")
            .EmailAddress()
            .WithMessage("Validation.EmailInvalid")
            .MaximumLength(200)
            .WithMessage("Validation.FieldMaxLength");
    }
}