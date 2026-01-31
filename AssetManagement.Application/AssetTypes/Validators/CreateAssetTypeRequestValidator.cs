using FluentValidation;
using AssetManagement.Application.AssetTypes.Dtos;

namespace AssetManagement.Application.AssetTypes.Validators;

public sealed class CreateAssetTypeRequestValidator : AbstractValidator<CreateAssetTypeRequest>
{
    public CreateAssetTypeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Validation.FieldRequired")
            .MaximumLength(60).WithMessage("Validation.FieldMaxLength");
    }
}