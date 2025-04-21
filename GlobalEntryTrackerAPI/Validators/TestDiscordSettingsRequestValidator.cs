using Business.Dto.Requests;
using FluentValidation;

namespace GlobalEntryTrackerAPI.Validators;

public class TestDiscordSettingsRequestValidator : AbstractValidator<TestDiscordSettingsRequest>
{
    public TestDiscordSettingsRequestValidator()
    {
        RuleFor(x => x.WebhookUrl)
            .NotEmpty()
            .WithMessage("Discord webhook URL is required.")
            .Matches("^https://discord\\.com/api/webhooks/")
            .WithMessage("Invalid Discord webhook URL format.");
    }
}