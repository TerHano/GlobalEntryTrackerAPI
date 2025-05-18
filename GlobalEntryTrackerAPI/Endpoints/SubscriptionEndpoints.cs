using Business;
using Business.Dto.Requests;
using GlobalEntryTrackerAPI.Extensions;

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
            });


        app.MapPost("/api/v1/subscribe",
            async (HttpContext httpContext,
                SubscriptionBusiness subscriptionBusiness,
                CreateCheckoutSessionRequest request) =>
            {
                var userId = httpContext.User.GetUserId();
                var sessionUrl =
                    await subscriptionBusiness.GetSubscriptionPaymentUrl(userId, request);
                return Results.Ok(sessionUrl);
            }).RequireAuthorization();

        app.MapPost("/api/v1/manage-subscription",
            async (HttpContext httpContext,
                SubscriptionBusiness subscriptionBusiness,
                ManageSubscriptionSessionRequest request) =>
            {
                var userId = httpContext.User.GetUserId();
                var sessionUrl =
                    await subscriptionBusiness.GetSubscriptionManagementUrl(userId, request);
                return Results.Ok(sessionUrl);
            }).RequireAuthorization();


        app.MapPatch("/api/v1/validate-subscription",
            async (HttpContext httpContext,
                SubscriptionBusiness subscriptionBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var isUserSubscribedAndActive =
                    await subscriptionBusiness.ValidatePurchaseForUser(userId);
                return Results.Ok(isUserSubscribedAndActive);
            }).RequireAuthorization();

        app.MapGet("/api/v1/subscription",
            async (HttpContext httpContext,
                SubscriptionBusiness subscriptionBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var subscriptionInformation =
                    await subscriptionBusiness.GetSubscriptionInformationForUser(userId);
                return Results.Ok(subscriptionInformation);
            }).RequireAuthorization();
    }
}