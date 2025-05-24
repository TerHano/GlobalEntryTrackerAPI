using Business;
using Business.Dto;
using Business.Dto.Requests;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Models;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class UserEndpoint
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        // GET: api/v1/me Get Current User's Details
        app.MapGet("/api/v1/me",
                async (HttpContext httpContext, UserBusiness userBusiness) =>
                {
                    var userId = httpContext.User.GetUserId();
                    var userDetails = await userBusiness.GetUserById(userId);
                    return Results.Ok(userDetails);
                })
            .RequireAuthorization()
            .WithTags("User")
            .WithName("GetCurrentUser")
            .WithSummary("Get current user's details")
            .WithDescription("Retrieves the details of the currently authenticated user.")
            .Produces<ApiResponse<UserDto>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        // GET: api/v1/me/permissions Get Current User's Permissions
        app.MapGet("/api/v1/me/permissions",
                async (HttpContext httpContext, UserBusiness userBusiness) =>
                {
                    var userId = httpContext.User.GetUserId();
                    var permissions = await userBusiness.GetPermissionsForUser(userId);
                    return Results.Ok(permissions);
                })
            .RequireAuthorization()
            .WithTags("User")
            .WithName("GetCurrentUserPermissions")
            .WithSummary("Get current user's permissions")
            .WithDescription(
                "Retrieves the permissions assigned to the currently authenticated user.")
            .Produces<ApiResponse<PermissionsDto>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        // PUT: api/v1/me Update Current User's Details
        app.MapPut("/api/v1/me",
                async (HttpContext httpContext, AuthBusiness authBusiness,
                    UpdateUserRequest request) =>
                {
                    var userId = httpContext.User.GetUserId();
                    await authBusiness.UpdateUser(request, userId);
                    return Results.Ok();
                })
            .RequireAuthorization()
            .WithTags("User")
            .WithName("UpdateCurrentUser")
            .WithSummary("Update current user's details")
            .WithDescription("Updates the details of the currently authenticated user.")
            .Accepts<UpdateUserRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        // GET: api/v1/next-notification Get Next Notification Check
        app.MapGet("/api/v1/next-notification",
                async (HttpContext httpContext, UserBusiness userBusiness) =>
                {
                    var userId = httpContext.User.GetUserId();

                    var nextNotificationCheck =
                        await userBusiness.GetNextNotificationCheckForUser(userId);
                    return Results.Ok(nextNotificationCheck);
                })
            .RequireAuthorization()
            .WithTags("User")
            .WithName("GetNextNotificationCheck")
            .WithSummary("Get next notification check for user")
            .WithDescription(
                "Retrieves the next scheduled notification check for the currently authenticated user.")
            .Produces<ApiResponse<DateTime>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}