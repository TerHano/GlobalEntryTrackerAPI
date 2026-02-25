using Business;

namespace GlobalEntryTrackerAPI.Endpoints.Webhooks;

public static class StripeWebhooks
{
    public static void MapStripeWebHooks(this WebApplication app)
    {
        app.MapPost("/webhook/v1/payment-success",
            async (HttpContext httpContext,
                SubscriptionBusiness subscriptionBusiness,
                ILoggerFactory loggerFactory
            ) =>
            {
                var logger = loggerFactory.CreateLogger("StripeWebhook");
                var stripeSignature = httpContext.Request.Headers["Stripe-Signature"];
                if (string.IsNullOrEmpty(stripeSignature))
                    return Results.BadRequest("Stripe signature is missing");

                const int maxBodySize = 1024 * 100; // 100KB
                if (httpContext.Request.ContentLength.GetValueOrDefault(0) > maxBodySize)
                    return Results.BadRequest("Request body too large");

                var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(json))
                    return Results.BadRequest("Request body is empty");

                try
                {
                    await subscriptionBusiness.HandleStripeWebhookEvents(
                        stripeSignature.ToString(),
                        json);
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing Stripe webhook event");
                    return Results.InternalServerError();
                }
            });
    }
}