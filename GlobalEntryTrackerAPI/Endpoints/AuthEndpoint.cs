using Business;
using Business.Dto.Requests;

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
            });
    }
}