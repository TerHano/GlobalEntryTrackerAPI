using GlobalEntryTrackerAPI.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace GlobalEntryTrackerAPI.Handlers;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception, "Exception occurred: {Message}", exception.Message);


        var response = new ApiResponse
        {
            Success = false,
            ErrorMessage = exception.Message
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response
            .WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}