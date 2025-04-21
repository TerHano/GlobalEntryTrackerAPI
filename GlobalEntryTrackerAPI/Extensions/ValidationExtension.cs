using FluentValidation;

namespace GlobalEntryTrackerAPI.Extensions;

public static class ValidationExtension
{
    public static async Task ValidateRequestAsync<T>(this T request, IValidator<T> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        await validator.ValidateAndThrowAsync(request);
    }
}