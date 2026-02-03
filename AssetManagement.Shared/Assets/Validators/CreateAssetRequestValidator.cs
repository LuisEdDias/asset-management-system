using AssetManagement.Shared.Assets.Dtos;
using FluentValidation;

namespace AssetManagement.Shared.Assets.Validators;

public sealed class CreateAssetRequestValidator : AbstractValidator<CreateAssetRequest>
{
    public CreateAssetRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Validation.FieldRequired")
            .MaximumLength(150)
            .WithMessage("Validation.FieldMaxLength");

        RuleFor(x => x.SerialNumber)
            .NotEmpty()
            .WithMessage("Validation.FieldRequired")
            .MaximumLength(100)
            .WithMessage("Validation.FieldMaxLength")
            .Matches(@"^[a-zA-Z0-9]*$")
            .WithMessage("Validation.SerialNumber");

        RuleFor(x => x.AssetTypeId)
            .GreaterThan(0)
            .WithMessage("Common.ValidationError");

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Asset.ValueGreaterThanOrZero");
    }
}