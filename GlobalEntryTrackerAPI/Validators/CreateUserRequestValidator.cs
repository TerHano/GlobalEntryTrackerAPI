using Business.Dto.Requests;
using FluentValidation;

namespace GlobalEntryTrackerAPI.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MinimumLength(2)
            .WithMessage("First name must be at least 2 characters long.");
        RuleFor(x => x.LastName).NotEmpty().MinimumLength(2)
            .WithMessage("Last name must be at least 2 characters long.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress()
            .WithMessage("A valid email address is required.");
    }
}