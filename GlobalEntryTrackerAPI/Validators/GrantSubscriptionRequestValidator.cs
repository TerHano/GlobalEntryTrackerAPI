using Business.Dto.Requests;
using FluentValidation;

namespace GlobalEntryTrackerAPI.Validators;

public class GrantSubscriptionRequestValidator : AbstractValidator<GrantSubscriptionRequest>
{
    public GrantSubscriptionRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.PriceId)
            .NotNull()
            .NotEmpty()
            .WithMessage("Price ID is required.")
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage(
                "Price ID must consist of alphanumeric characters, underscores, or hyphens.");
    }
}
