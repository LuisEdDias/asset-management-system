using FluentValidation;
using AssetManagement.Shared.AssetTypes.Dtos;

namespace AssetManagement.Shared.AssetTypes.Validators;

public sealed class UpdateAssetTypeRequestValidator : AbstractValidator<UpdateAssetTypeRequest>
{
    public UpdateAssetTypeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Validation.FieldRequired")
            .MaximumLength(60).WithMessage("Validation.FieldMaxLength")
            .Matches(@"^[a-zA-ZÀ-ÿ ]+$")
            .WithMessage("Validation.Name")
            .Must(name => !name.Contains("  "))
            .WithMessage("Validation.Name");
    }
}