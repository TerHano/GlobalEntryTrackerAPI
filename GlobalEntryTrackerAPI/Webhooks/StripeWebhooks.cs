using Business;

namespace GlobalEntryTrackerAPI.Webhooks;

public static class StripeWebhooks
{
    public static void MapStripeWebHooks(this WebApplication app)
    {
        app.MapPost("/webhook/v1/payment-success",
            async (HttpContext httpContext,
                SubscriptionBusiness subscriptionBusiness
            ) =>
            {
                var stripeSignature = httpContext.Request.Headers["Stripe-Signature"];
                if (string.IsNullOrEmpty(stripeSignature))
                    return Results.BadRequest("Stripe signature is missing");
                var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
                try
                {
                    await subscriptionBusiness.HandleStripeWebhookEvents(
                        stripeSignature.ToString(),
                        json);
                    return Results.Ok();
                }
                catch (Exception)
                {
                    return Results.BadRequest();
                }
            });
    }
}