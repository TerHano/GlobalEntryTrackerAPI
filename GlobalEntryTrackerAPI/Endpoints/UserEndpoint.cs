using Business;
using GlobalEntryTrackerAPI.Extensions;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class UserEndpoint
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        // GET: api/v1/me Get Currents Users Details
        app.MapGet("/api/v1/me",
            async (HttpContext httpContext, UserBusiness userBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var userDetails = await userBusiness.GetUserById(userId);
                return Results.Ok(userDetails);
            }).RequireAuthorization();
    }
}