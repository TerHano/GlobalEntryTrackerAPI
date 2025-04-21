using Business;
using Business.Dto.Requests;
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

        // GET: api/v1/me/permissions Get Current Users Permissions
        app.MapGet("/api/v1/me/permissions",
            async (HttpContext httpContext, UserBusiness userBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var permissions = await userBusiness.GetPermissionsForUser(userId);
                return Results.Ok(permissions);
            }).RequireAuthorization();

        // POST: api/v1/me Update Current Users Details
        app.MapPut("/api/v1/me",
            async (HttpContext httpContext, UserBusiness userBusiness, UpdateUserRequest request) =>
            {
                var userId = httpContext.User.GetUserId();
                await userBusiness.UpdateUser(request, userId);
                return Results.Ok();
            }).RequireAuthorization();
    }
}