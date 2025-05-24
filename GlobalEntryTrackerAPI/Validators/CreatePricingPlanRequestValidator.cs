using Business.Dto.Requests;
using FluentValidation;

namespace GlobalEntryTrackerAPI.Validators;

public class CreatePricingPlanRequestValidator : AbstractValidator<CreatePricingPlanRequest>
{
    public CreatePricingPlanRequestValidator()
    {
        RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty()
            .WithMessage("Description is required.");
        RuleFor(x => x.PriceId)
            .NotNull()
            .NotEmpty()
            .WithMessage("Price ID is required.")
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage(
                "Price ID must consist of alphanumeric characters, underscores, or hyphens.");
        RuleFor(x => x.Features)
            .NotNull()
            .NotEmpty()
            .WithMessage("Features are required.");
    }
}