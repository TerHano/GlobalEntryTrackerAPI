// GlobalEntryTrackerAPI/Endpoints/SubscriptionEndpoints.cs

using Business;
using Business.Dto;
using Business.Dto.Requests;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Models;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class SubscriptionEndpoints
{
    public static void MapSubscriptionEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/plans",
                async (PlanBusiness planBusiness) =>
                {
                    var plans = await planBusiness.GetPlanOptions();
                    return Results.Ok(plans);
                })
            .WithTags("Subscription")
            .WithName("GetPlans")
            .WithSummary("Get available subscription plans")
            .WithDescription("Retrieves a list of available subscription plans for users.")
            .Produces<ApiResponse<PlanOptionDto[]>>();

        app.MapPost("/api/v1/subscribe",
                async (HttpContext httpContext,
                    SubscriptionBusiness subscriptionBusiness,
                    CreateCheckoutSessionRequest request) =>
                {
                    var userId = httpContext.User.GetUserId();
                    var sessionUrl =
                        await subscriptionBusiness.GetSubscriptionPaymentUrl(userId, request);
                    return Results.Ok(sessionUrl);
                })
            .RequireAuthorization()
            .WithTags("Subscription")
            .WithName("Subscribe")
            .WithSummary("Subscribe to a plan")
            .WithDescription("Creates a checkout session for the user to subscribe to a plan.")
            .Accepts<CreateCheckoutSessionRequest>("application/json")
            .Produces<ApiResponse<string>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapPost("/api/v1/manage-subscription",
                async (HttpContext httpContext,
                    SubscriptionBusiness subscriptionBusiness,
                    ManageSubscriptionSessionRequest request) =>
                {
                    var userId = httpContext.User.GetUserId();
                    var sessionUrl =
                        await subscriptionBusiness.GetSubscriptionManagementUrl(userId, request);
                    return Results.Ok(sessionUrl);
                })
            .RequireAuthorization()
            .WithTags("Subscription")
            .WithName("ManageSubscription")
            .WithSummary("Manage subscription")
            .WithDescription("Creates a session for the user to manage their subscription.")
            .Accepts<ManageSubscriptionSessionRequest>("application/json")
            .Produces<ApiResponse<string>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapPatch("/api/v1/validate-subscription",
                async (HttpContext httpContext,
                    SubscriptionBusiness subscriptionBusiness) =>
                {
                    var userId = httpContext.User.GetUserId();
                    var isUserSubscribedAndActive =
                        await subscriptionBusiness.ValidatePurchaseForUser(userId);
                    return Results.Ok(isUserSubscribedAndActive);
                })
            .RequireAuthorization()
            .WithTags("Subscription")
            .WithName("ValidateSubscription")
            .WithSummary("Validate user subscription")
            .WithDescription("Validates if the current user has an active subscription.")
            .Produces<ApiResponse<bool>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapGet("/api/v1/subscription",
                async (HttpContext httpContext,
                    SubscriptionBusiness subscriptionBusiness) =>
                {
                    var userId = httpContext.User.GetUserId();
                    var subscriptionInformation =
                        await subscriptionBusiness.GetSubscriptionInformationForUser(userId);
                    return Results.Ok(subscriptionInformation);
                })
            .RequireAuthorization()
            .WithTags("Subscription")
            .WithName("GetSubscriptionInfo")
            .WithSummary("Get subscription information")
            .WithDescription("Retrieves the subscription information for the current user.")
            .Produces<ApiResponse<UserSubscriptionDto>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}