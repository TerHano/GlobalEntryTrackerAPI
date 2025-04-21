using Business.Dto.Requests;
using FluentValidation;

namespace GlobalEntryTrackerAPI.Validators;

public class CreateTrackerForUserRequestValidator : AbstractValidator<CreateTrackerForUserRequest>
{
    public CreateTrackerForUserRequestValidator()
    {
        RuleFor(x => x.Enabled).NotNull();
        RuleFor(x => x.LocationId).NotNull();
        RuleFor(x => x.NotificationTypeId).NotNull();
        RuleFor(x => x.CutOffDate).NotNull();
    }
}