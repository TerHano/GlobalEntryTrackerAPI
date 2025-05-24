using Business;
using Business.Dto.Requests;
using GlobalEntryTrackerAPI.Models;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class AuthEndpoint
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/sign-up",
                async (CreateUserRequest request, UserBusiness userBusiness) =>
                {
                    await userBusiness.CreateUser(request);
                    return Results.Ok();
                })
            .WithTags("Authentication")
            .WithName("SignUp")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account with the provided registration details.")
            .Accepts<CreateUserRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
    }
}