using Business;
using Business.Dto.Admin;
using Business.Dto.Requests;
using FluentValidation;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Models;
using GlobalEntryTrackerAPI.Validators;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        // Get: All users
        app.MapGet("/api/v1/admin/users",
                async (UserBusiness userBusiness, HttpContext httpContext) =>
                {
                    var userId = httpContext.User.GetUserId();
                    var users = await userBusiness.GetAllUsersForAdmin(userId);
                    return Results.Ok(users);
                })
            .RequireAuthorization("Admin")
            .WithTags("Admin")
            .WithName("GetAllUsers")
            .WithSummary("Get all users")
            .WithDescription(
                "Retrieves a list of all users in the system. Requires Admin authorization.")
            .Produces<ApiResponse<UserDtoForAdmin[]>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);


        //Get : All roles
        app.MapGet("/api/v1/admin/roles",
                async (RoleBusiness roleBusiness) =>
                {
                    var roles = await roleBusiness.GetAllRoles();
                    return Results.Ok(roles);
                })
            .RequireAuthorization("Admin")
            .WithTags("Admin")
            .WithName("GetAllRoles")
            .WithSummary("Get all roles")
            .WithDescription(
                "Retrieves a list of all roles in the system. Requires Admin authorization.")
            .Produces<ApiResponse<RoleDto[]>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);

        // Delete: User by userId
        app.MapDelete("/api/v1/admin/user/{userId}",
                async (string userId, AuthBusiness authBusiness) =>
                {
                    var userIdInt = int.Parse(userId);
                    await authBusiness.DeleteUserById(userIdInt);
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

        app.MapPost("/api/v1/admin/pricing",
                async (CreatePricingPlanRequest request, PlanBusiness planBusiness) =>
                {
                    var validator = new CreatePricingPlanRequestValidator();
                    await validator.ValidateAndThrowAsync(request);
                    var planId = await planBusiness.AddPlanOption(request);
                    return Results.Ok(planId);
                })
            .RequireAuthorization("Admin")
            .WithTags("Admin")
            .WithName("AddPricing")
            .WithSummary("Add pricing to the system")
            .WithDescription(
                "Adds pricing information to the system. Requires Admin authorization.")
            .Produces<ApiResponse<int>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);

        app.MapPut("/api/v1/admin/pricing",
                async (UpdatePricingPlanRequest request, PlanBusiness planBusiness) =>
                {
                    var validator = new UpdatePricingPlanRequestValidator();
                    await validator.ValidateAndThrowAsync(request);
                    await planBusiness.UpdatePlanOption(request);
                    return Results.Ok();
                })
            .RequireAuthorization("Admin")
            .WithTags("Admin")
            .WithName("UpdatePricing")
            .WithSummary("Update pricing to the system")
            .WithDescription(
                "Updates pricing information to the system. Requires Admin authorization.")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);

        app.MapDelete("/api/v1/admin/pricing/{id}",
                async (int id, PlanBusiness planBusiness) =>
                {
                    await planBusiness.DeletePlanOption(id);
                    return Results.Ok();
                })
            .RequireAuthorization("Admin")
            .WithTags("Admin")
            .WithName("DeletePricing")
            .WithSummary("Deletes pricing from the system")
            .WithDescription(
                "Deletes pricing information from the system by its ID. Requires Admin authorization.")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);

        app.MapPost("/api/v1/admin/grant-subscription",
                async (SubscriptionBusiness subscriptionBusiness,
                    GrantSubscriptionRequest request) =>
                {
                    await subscriptionBusiness.GrantSubscriptionToUser(request);
                    return Results.Ok();
                })
            .RequireAuthorization("Admin")
            .WithTags("Admin")
            .WithName("GrantSubscription")
            .WithSummary("Grant a subscription to a user")
            .WithDescription(
                "Grants a subscription to a user by their userId and planId. Requires Admin authorization.")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);
    }
}