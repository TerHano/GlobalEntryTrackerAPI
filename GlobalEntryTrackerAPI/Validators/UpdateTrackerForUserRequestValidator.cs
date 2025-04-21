using Business.Dto.Requests;
using FluentValidation;

namespace GlobalEntryTrackerAPI.Validators;

public class UpdateTrackerForUserRequestValidator : AbstractValidator<UpdateTrackerForUserRequest>
{
    public UpdateTrackerForUserRequestValidator()
    {
        RuleFor(x => x.Id).NotNull();
        RuleFor(x => x.Enabled).NotNull();
        RuleFor(x => x.LocationId).NotNull();
        RuleFor(x => x.NotificationTypeId).NotNull();
        RuleFor(x => x.CutOffDate).NotNull();
    }
}