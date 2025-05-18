using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;
using Database.Enums;
using Database.Repositories;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace Business;

public class SubscriptionBusiness(
    UserCustomerRepository userCustomerRepository,
    UserRoleRepository userRoleRepository,
    ILogger<SubscriptionBusiness> logger)
{
    public async Task<string> GetSubscriptionPaymentUrl(int userId,
        CreateCheckoutSessionRequest request)
    {
        var options = new SessionCreateOptions
        {
            LineItems =
            [
                new SessionLineItemOptions
                {
                    // Provide the exact Price ID (for example, price_1234) of the product you want to sell
                    Price = "price_1RPXk2Gf0lE9KRaJ0pdDawKN",
                    Quantity = 1
                }
            ],
            CustomerCreation = "always",
            Mode = "subscription",
            AllowPromotionCodes = true,
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId.ToString() }
                }
            }
        };
        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }

    public async Task<string> GetSubscriptionManagementUrl(int userId,
        ManageSubscriptionSessionRequest request)
    {
        var userCustomer = await userCustomerRepository.GetCustomerDetailsForUser(userId);
        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = userCustomer.CustomerId,
            ReturnUrl = request.ReturnUrl
        };
        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }

    public async Task<bool> ValidatePurchaseForUser(int userId)
    {
        var userCustomer = await userCustomerRepository.GetCustomerDetailsForUser(userId);
        var service = new SubscriptionService();
        var subscription = await service.GetAsync(userCustomer.SubscriptionId);
        //User has a valid and active subscription
        if (subscription.Status.Equals("active"))
        {
            await userRoleRepository.AddRoleForUser(userId, Role.Subscriber);
            return true;
        }

        //User does not have a valid subscription
        return false;
    }

    public async Task<UserSubscriptionDto> GetSubscriptionInformationForUser(int userId)
    {
        var userCustomer = await userCustomerRepository.GetCustomerDetailsForUser(userId);
        var service = new SubscriptionService();
        var subscription = await service.GetAsync(userCustomer.SubscriptionId);
        var paymentId = subscription.DefaultPaymentMethodId;

        //If paymentId is null, grab from Customer
        if (paymentId == null)
        {
            var customerService = new CustomerService();
            var customer = await customerService.GetAsync(userCustomer.CustomerId);
            paymentId = customer.InvoiceSettings.DefaultPaymentMethodId;
        }

        var paymentService = new PaymentMethodService();
        var paymentMethodAsync = paymentService.GetAsync(paymentId);
        var latestSubscriptionItem = subscription.Items.Data.FirstOrDefault();
        if (latestSubscriptionItem == null)
            throw new NullReferenceException("Subscription item not found");

        var subscriptionEndDate =
            subscription.CancelAt.HasValue
                ? DateOnly.FromDateTime(subscription.CancelAt.Value).AddDays(1)
                : DateOnly.FromDateTime(latestSubscriptionItem.CurrentPeriodEnd).AddDays(1);


        var paymentMethod = await paymentMethodAsync;
        var card = paymentMethod.Card;

        //Get Product Info
        var productService = new ProductService();
        var product = await productService.GetAsync(latestSubscriptionItem.Plan.ProductId);
        if (product == null)
            throw new NullReferenceException("Product not found");
        //Get Product Metadata

        var userSubscription = new UserSubscriptionDto
        {
            Active = subscription.Status.Equals("active"),
            PlanName = product.Name,
            PlanPrice = latestSubscriptionItem.Price.UnitAmount ?? 0,
            Currency = latestSubscriptionItem.Price.Currency,
            PlanInterval = latestSubscriptionItem.Price.Recurring.Interval,
            NextPaymentDate =
                subscriptionEndDate,
            IsEnding = subscription.CanceledAt != null,
            CardLast4Digits = card.Last4,
            CardBrand = card.Brand
        };
        return userSubscription;
    }

    public async Task HandleStripeWebhookEvents(string stripeSignature,
        string jsonBody)
    {
        var stripeWebhookSecret =
            Environment.GetEnvironmentVariable("Stripe__WebhookSecret");

        //Get request body
        try
        {
            var stripeEvent =
                EventUtility.ConstructEvent(jsonBody, stripeSignature, stripeWebhookSecret);
            if (stripeEvent.Type == EventTypes.CustomerSubscriptionCreated)
            {
                if (stripeEvent.Data.Object is not Subscription subscriptionEvent)
                {
                    logger.LogError("Subscription event is null");
                    throw new NullReferenceException("Subscription event is null");
                }

                var userId = subscriptionEvent.Metadata["userId"];
                if (userId == null)
                {
                    logger.LogError("UserId not found in subscription metadata");
                    throw new NullReferenceException("UserId not found in subscription metadata");
                }

                var userIdInt = int.Parse(userId);
                var userCustomer = new UserCustomerEntity
                {
                    UserId = userIdInt,
                    SubscriptionId = subscriptionEvent.Id,
                    CustomerId = subscriptionEvent.CustomerId
                };
                await userRoleRepository.AddRoleForUser(userIdInt, Role.Subscriber);
                await userCustomerRepository.AddUpdateUserCustomer(userCustomer);
            }
            else if (stripeEvent.Type == EventTypes.CustomerSubscriptionDeleted)
            {
                if (stripeEvent.Data.Object is not Subscription subscriptionEvent)
                {
                    logger.LogError("Subscription event is null");
                    throw new NullReferenceException("Subscription event is null");
                }

                var userId = subscriptionEvent.Metadata["userId"];
                if (userId == null)
                {
                    logger.LogError("UserId not found in subscription metadata");
                    throw new NullReferenceException("UserId not found in subscription metadata");
                }

                var userIdInt = int.Parse(userId);
                await userRoleRepository.RemoveRoleForUser(userIdInt, Role.Subscriber);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            throw new ApplicationException(
                "Error processing subscription event, please try again later");
        }
    }
}