using Business.Dto.Requests;
using FluentValidation;

namespace GlobalEntryTrackerAPI.Validators;

public class UpdateDiscordSettingsRequestValidator : AbstractValidator<UpdateDiscordSettingsRequest>
{
    public UpdateDiscordSettingsRequestValidator()
    {
        RuleFor(x => x.Id).NotNull();
        RuleFor(x => x.WebhookUrl)
            .NotEmpty()
            .WithMessage("Discord webhook URL is required.")
            .Matches("^https://discord\\.com/api/webhooks/")
            .WithMessage("Invalid Discord webhook URL format.");

        RuleFor(x => x.Enabled)
            .NotNull()
            .WithMessage("Enabled status is required.")
            .Must(x => x is true or false)
            .WithMessage("Enabled status must be a boolean value.");
    }
}