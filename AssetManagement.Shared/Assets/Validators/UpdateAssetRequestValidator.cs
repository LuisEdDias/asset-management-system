using AssetManagement.Shared.Assets.Dtos;
using FluentValidation;

namespace AssetManagement.Shared.Assets.Validators;

public sealed class UpdateAssetRequestValidator : AbstractValidator<UpdateAssetRequest>
{
    public UpdateAssetRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Validation.FieldRequired")
            .MaximumLength(150)
            .WithMessage("Validation.FieldMaxLength");

        RuleFor(x => x.AssetTypeId)
            .GreaterThan(0)
            .WithMessage("Common.ValidationError");

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Asset.ValueGreaterThanOrZero");
    }
}