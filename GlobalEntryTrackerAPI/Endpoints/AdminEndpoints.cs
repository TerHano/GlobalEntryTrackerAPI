using Business;

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
            }).RequireAuthorization("Admin");
    }
}