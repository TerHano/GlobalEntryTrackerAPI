using Business;

namespace GlobalEntryTrackerAPI.Webhooks;

public static class StripeWebhooks
{
    public static void MapStripeWebHooks(this WebApplication app)
    {
        //Add logging to the webhook
        app.MapPost("/webhook/v1/payment-success",
            async (HttpContext httpContext,
                SubscriptionBusiness subscriptionBusiness
            ) =>
            {
                var stripeSignature = httpContext.Request.Headers["Stripe-Signature"];
                //Get request body
                var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
                try
                {
                    await subscriptionBusiness.HandleStripeWebhookEvents(
                        stripeSignature,
                        json);
                    return Results.Ok();
                }
                catch (Exception e)
                {
                    return Results.BadRequest();
                }
            });
    }
}