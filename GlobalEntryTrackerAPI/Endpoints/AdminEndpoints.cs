using Business;
using GlobalEntryTrackerAPI.Models;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        // Delete: User by userId
        app.MapDelete("/api/v1/admin/user/{userId}",
                async (string userId, UserBusiness userBusiness) =>
                {
                    var userIdInt = int.Parse(userId);
                    await userBusiness.DeleteUserById(userIdInt);
                    return Results.Ok();
                })
            .RequireAuthorization("Admin")
            .WithTags("Admin")
            .WithName("DeleteUserById")
            .WithSummary("Delete a user by userId")
            .WithDescription(
                "Deletes a user from the system by their userId. Requires Admin authorization.")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);
    }
}