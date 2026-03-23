using Business;
using Business.Dto;
using Business.Dto.Requests;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Models;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class AuthEndpoint
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/auth/v1/authenticated",
                (
                    HttpContext httpContext) =>
                {
                    try
                    {
                        httpContext.User.GetUserId();
                    }
                    catch (Exception)
                    {
                        return Results.Ok(new IsAuthenticatedDto
                        {
                            IsAuthenticated = false
                        });
                    }

                    return Results.Ok(new IsAuthenticatedDto
                    {
                        IsAuthenticated = true
                    });
                })
            .WithTags("Authentication")
            .WithName("IsAuthenticated")
            .WithSummary("Check if user is authenticated")
            .WithDescription("Checks if the user is authenticated and returns a boolean value.")
            .Produces<ApiResponse<IsAuthenticatedDto>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        app.MapPost("/api/auth/v1/password-recovery",
                async (IAuthBusiness authBusiness, PasswordResetEmailRequest request) =>
                {
                    await authBusiness.SendPasswordResetEmail(request);
                    return Results.Ok();
                })
            .WithTags("Authentication")
            .WithName("SendPasswordResetEmail")
            .WithSummary("Send password reset email")
            .WithDescription(
                "Sends a password reset email to the user with the provided email address.")
            .Accepts<PasswordResetEmailRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
    }
}