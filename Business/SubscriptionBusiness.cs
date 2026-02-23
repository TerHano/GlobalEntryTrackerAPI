using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;
using Database.Enums;
using Database.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service;
using Stripe;
using Stripe.Checkout;

namespace Business;

/// <summary>
///     Handles business logic for user subscriptions and Stripe integration.
/// </summary>
public class SubscriptionBusiness(
    UserCustomerRepository userCustomerRepository,
    StripeWebhookEventRepository stripeWebhookEventRepository,
    UserRoleRepository userRoleRepository,
    UserProfileRepository userProfileRepository,
    PlanOptionRepository planOptionRepository,
    RoleManager<RoleEntity> roleManager,
    UserRoleService userRoleService,
    IConfiguration configuration,
    ILogger<SubscriptionBusiness> logger)
{
    /// <summary>
    ///     Creates a Stripe Checkout session for a subscription payment.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="request">Checkout session request.</param>
    /// <returns>Stripe Checkout session URL.</returns>
    public async Task<string> GetSubscriptionPaymentUrl(string userId,
        CreateCheckoutSessionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PriceId)) throw new Exception("Price Id is required");
        if (!IsAllowedRedirectUrl(request.SuccessUrl) || !IsAllowedRedirectUrl(request.CancelUrl))
            throw new Exception("Invalid checkout redirect URL.");

        var availablePlans = await planOptionRepository.GetAllPlanOptions();
        var selectedPlan = availablePlans.FirstOrDefault(x =>
            x.PriceId.Equals(request.PriceId, StringComparison.Ordinal));
        if (selectedPlan == null)
            throw new Exception("Requested price is not available.");

        var existingUserCustomer = await userCustomerRepository.GetCustomerDetailsForUser(userId);

        var options = new SessionCreateOptions
        {
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = selectedPlan.PriceId,
                    Quantity = 1
                }
            ],

            Mode = "subscription",
            AllowPromotionCodes = true,
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            Customer = existingUserCustomer?.CustomerId,
            ClientReferenceId = userId,
            Metadata = new Dictionary<string, string>
            {
                { "userId", userId }
            },
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId }
                }
            }
        };
        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }

    /// <summary>
    ///     Gets the Stripe subscription management portal URL for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="request">Management session request.</param>
    /// <returns>Stripe Billing Portal session URL.</returns>
    public async Task<string> GetSubscriptionManagementUrl(string userId,
        ManageSubscriptionSessionRequest request)
    {
        if (!IsAllowedRedirectUrl(request.ReturnUrl))
            throw new Exception("Invalid management return URL.");

        var userCustomer = await userCustomerRepository.GetCustomerDetailsForUser(userId);
        if (userCustomer == null)
            throw new NullReferenceException("User customer not found");
        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = userCustomer.CustomerId,
            ReturnUrl = request.ReturnUrl
        };
        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }

    /// <summary>
    ///     Validates if a user has an active subscription and updates their role.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>True if the user has an active subscription; otherwise, false.</returns>
    public async Task<bool> ValidatePurchaseForUser(string userId)
    {
        var userCustomer = await userCustomerRepository.GetCustomerDetailsForUser(userId);
        if (userCustomer == null)
            return false;
        var service = new SubscriptionService();
        var subscription = await service.GetAsync(userCustomer.SubscriptionId);
        //User has a valid and active subscription
        if (subscription.Status.Equals("active") || subscription.Status.Equals(
                "trialing"))
        {
            await userRoleRepository.AddEditRoleForUser(userId, Role.Subscriber);
            return true;
        }

        //User does not have a valid subscription
        return false;
    }

    /// <summary>
    ///     Retrieves subscription information for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>User subscription DTO or null.</returns>
    public async Task<UserSubscriptionDto?> GetSubscriptionInformationForUser(string userId)
    {
        var freePlanInformation = new UserSubscriptionDto
        {
            ActiveBilledSubscription = false,
            PlanName = "Free",
            PlanPrice = 0,
            Currency = "USD",
            PlanInterval = "month",
            NextPaymentDate =
                null,
            IsEnding = false,
            CardLast4Digits = null,
            CardBrand = null
        };

        var friendsAndFamilyPlan = new UserSubscriptionDto
        {
            ActiveBilledSubscription = true,
            PlanName = "Friends & Family",
            PlanPrice = 0,
            Currency = "USD",
            PlanInterval = "month",
            NextPaymentDate =
                null,
            IsEnding = false,
            CardLast4Digits = null,
            CardBrand = null
        };

        var userRoles = await userRoleRepository.GetUserRoleByUserId(userId);
        if (userRoles.Any(x => x.Code == Role.FriendsFamily.GetCode()))
            return friendsAndFamilyPlan;
        var userCustomer = await userCustomerRepository.GetCustomerDetailsForUser(userId);
        if (userCustomer == null)
            return freePlanInformation;
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

        Task<PaymentMethod?> paymentMethodAsync;

        if (paymentId == null)
        {
            paymentMethodAsync = Task.FromResult<PaymentMethod?>(null);
        }
        else
        {
            var paymentService = new PaymentMethodService();
            paymentMethodAsync = paymentService.GetAsync(paymentId);
        }

        var latestSubscriptionItem = subscription.Items.Data.FirstOrDefault();
        if (latestSubscriptionItem == null)
            throw new NullReferenceException("Subscription item not found");

        var subscriptionEndDate =
            subscription.CancelAt.HasValue
                ? DateOnly.FromDateTime(subscription.CancelAt.Value).AddDays(1)
                : DateOnly.FromDateTime(latestSubscriptionItem.CurrentPeriodEnd).AddDays(1);


        var paymentMethod = await paymentMethodAsync;
        var card = paymentMethod?.Card;

        //Get Product Info
        var productService = new ProductService();
        var product = await productService.GetAsync(latestSubscriptionItem.Plan.ProductId);
        if (product == null)
            throw new NullReferenceException("Product not found");
        //Get Product Metadata
        var subssciptionStatus = subscription.Status;
        if (subssciptionStatus.Equals("canceled")) return freePlanInformation;

        var userSubscription = new UserSubscriptionDto
        {
            ActiveBilledSubscription = subssciptionStatus == "active",
            PlanName = product.Name,
            PlanPrice = latestSubscriptionItem.Price.UnitAmount ?? 0,
            Currency = latestSubscriptionItem.Price.Currency,
            PlanInterval = latestSubscriptionItem.Price.Recurring.Interval,
            NextPaymentDate =
                subscriptionEndDate,
            IsEnding = subscription.CanceledAt != null,
            CardLast4Digits = card?.Last4,
            CardBrand = card?.Brand
        };
        return userSubscription;
    }

    /// <summary>
    ///     Handles Stripe webhook events for subscription creation and deletion.
    /// </summary>
    /// <param name="stripeSignature">Stripe signature header.</param>
    /// <param name="jsonBody">Raw JSON body of the webhook event.</param>
    public async Task HandleStripeWebhookEvents(string stripeSignature,
        string jsonBody)
    {
        var stripeWebhookSecret =
            configuration["Stripe:Webhook_Secret"];

        if (string.IsNullOrWhiteSpace(stripeWebhookSecret))
            throw new ApplicationException("Stripe webhook secret is missing.");

        string? stripeEventId = null;

        //Get request body
        try
        {
            var stripeEvent =
                EventUtility.ConstructEvent(jsonBody, stripeSignature, stripeWebhookSecret);

            stripeEventId = stripeEvent.Id;
            if (string.IsNullOrWhiteSpace(stripeEventId))
                throw new ApplicationException("Stripe webhook event id is missing.");

            var isNewEvent = await stripeWebhookEventRepository.TryStartProcessing(
                stripeEventId,
                stripeEvent.Type);
            if (!isNewEvent)
            {
                logger.LogInformation(
                    "Duplicate Stripe webhook event ignored: {StripeEventId} ({StripeEventType})",
                    stripeEventId,
                    stripeEvent.Type);
                return;
            }

            switch (stripeEvent.Type)
            {
                case EventTypes.CustomerSubscriptionCreated:
                {
                    if (stripeEvent.Data.Object is not Subscription subscriptionEvent)
                    {
                        logger.LogError("Subscription event is null");
                        throw new NullReferenceException("Subscription event is null");
                    }

                    var userId = await ResolveUserId(subscriptionEvent);
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        logger.LogWarning("UserId not found while processing subscription created event");
                        break;
                    }


                    var userCustomer = new UserCustomerEntity
                    {
                        UserId = userId,
                        SubscriptionId = subscriptionEvent.Id,
                        CustomerId = subscriptionEvent.CustomerId
                    };
                    await userCustomerRepository.AddEditUserCustomer(userCustomer);
                    await ActivateSubscriptionForUser(userId);

                    break;
                }
                case EventTypes.CustomerSubscriptionUpdated:
                {
                    if (stripeEvent.Data.Object is not Subscription subscriptionEvent)
                    {
                        logger.LogError("Subscription event is null");
                        throw new NullReferenceException("Subscription event is null");
                    }

                    var userId = await ResolveUserId(subscriptionEvent);
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        logger.LogWarning("UserId not found while processing subscription updated event");
                        break;
                    }

                    await userCustomerRepository.AddEditUserCustomer(new UserCustomerEntity
                    {
                        UserId = userId,
                        SubscriptionId = subscriptionEvent.Id,
                        CustomerId = subscriptionEvent.CustomerId
                    });

                    if (IsSubscriptionActive(subscriptionEvent.Status))
                    {
                        await ActivateSubscriptionForUser(userId);
                    }
                    else
                    {
                        await userRoleRepository.RemoveRoleForUser(userId, Role.Subscriber);
                    }

                    break;
                }
                case EventTypes.CheckoutSessionCompleted:
                {
                    if (stripeEvent.Data.Object is not Session checkoutSession)
                    {
                        logger.LogError("Checkout session event is null");
                        throw new NullReferenceException("Checkout session event is null");
                    }

                    var userId = ResolveUserId(checkoutSession);
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        logger.LogWarning("UserId not found while processing checkout.session.completed event");
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(checkoutSession.CustomerId) &&
                        !string.IsNullOrWhiteSpace(checkoutSession.SubscriptionId))
                    {
                        await userCustomerRepository.AddEditUserCustomer(new UserCustomerEntity
                        {
                            UserId = userId,
                            CustomerId = checkoutSession.CustomerId,
                            SubscriptionId = checkoutSession.SubscriptionId
                        });
                    }

                    if (checkoutSession.PaymentStatus == "paid")
                    {
                        await ActivateSubscriptionForUser(userId);
                    }

                    break;
                }
                case EventTypes.InvoicePaid:
                {
                    if (stripeEvent.Data.Object is not Invoice invoice)
                    {
                        logger.LogError("Invoice event is null");
                        throw new NullReferenceException("Invoice event is null");
                    }

                    if (string.IsNullOrWhiteSpace(invoice.CustomerId))
                    {
                        logger.LogWarning("Invoice paid event does not include a customer id");
                        break;
                    }

                    var userCustomer =
                        await userCustomerRepository.GetCustomerDetailsForCustomerId(invoice.CustomerId);
                    if (userCustomer == null)
                    {
                        logger.LogWarning("User customer mapping not found for invoice paid event");
                        break;
                    }

                    await ActivateSubscriptionForUser(userCustomer.UserId);
                    break;
                }
                case EventTypes.InvoicePaymentFailed:
                {
                    if (stripeEvent.Data.Object is not Invoice invoice)
                    {
                        logger.LogError("Invoice event is null");
                        throw new NullReferenceException("Invoice event is null");
                    }

                    logger.LogWarning("Invoice payment failed for customer {CustomerId}",
                        invoice.CustomerId);
                    break;
                }
                case EventTypes.CustomerSubscriptionDeleted:
                {
                    if (stripeEvent.Data.Object is not Subscription subscriptionEvent)
                    {
                        logger.LogError("Subscription event is null");
                        throw new NullReferenceException("Subscription event is null");
                    }

                    var userId = await ResolveUserId(subscriptionEvent);
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        logger.LogWarning("UserId not found while processing subscription deleted event");
                        break;
                    }

                    await userRoleRepository.RemoveRoleForUser(userId, Role.Subscriber);
                    break;
                }
                default:
                {
                    logger.LogInformation("Unhandled Stripe event type {StripeEventType}",
                        stripeEvent.Type);
                    break;
                }
            }

            await stripeWebhookEventRepository.MarkProcessed(stripeEventId);
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(stripeEventId))
            {
                try
                {
                    await stripeWebhookEventRepository.ReleaseProcessingLock(stripeEventId);
                }
                catch (Exception lockReleaseException)
                {
                    logger.LogWarning(lockReleaseException,
                        "Failed to release Stripe webhook processing lock for event {StripeEventId}",
                        stripeEventId);
                }
            }

            logger.LogError(ex, "Error processing Stripe webhook event");
            throw new ApplicationException(
                "Error processing subscription event, please try again later");
        }
    }

    private async Task ActivateSubscriptionForUser(string userId)
    {
        await userRoleRepository.AddEditRoleForUser(userId, Role.Subscriber);

        var user = await userProfileRepository.GetUserProfileById(userId, true);
        if (user == null)
        {
            logger.LogWarning("User profile not found for user id {UserId}", userId);
            return;
        }

        var roles = roleManager.Roles.ToList();
        userRoleService.UpdateNextNotificationTimeForUser(user, roles);
        await userProfileRepository.UpdateUserProfile(user);
    }

    private async Task<string?> ResolveUserId(Subscription subscription)
    {
        if (subscription.Metadata != null &&
            subscription.Metadata.TryGetValue("userId", out var metadataUserId) &&
            !string.IsNullOrWhiteSpace(metadataUserId))
            return metadataUserId;

        if (!string.IsNullOrWhiteSpace(subscription.CustomerId))
        {
            var existingUserCustomer =
                await userCustomerRepository.GetCustomerDetailsForCustomerId(subscription.CustomerId);
            if (existingUserCustomer != null)
                return existingUserCustomer.UserId;
        }

        if (!string.IsNullOrWhiteSpace(subscription.Id))
        {
            var existingUserCustomer =
                await userCustomerRepository.GetCustomerDetailsForSubscriptionId(subscription.Id);
            if (existingUserCustomer != null)
                return existingUserCustomer.UserId;
        }

        return null;
    }

    private static string? ResolveUserId(Session checkoutSession)
    {
        if (checkoutSession.Metadata != null &&
            checkoutSession.Metadata.TryGetValue("userId", out var metadataUserId) &&
            !string.IsNullOrWhiteSpace(metadataUserId))
            return metadataUserId;

        return string.IsNullOrWhiteSpace(checkoutSession.ClientReferenceId)
            ? null
            : checkoutSession.ClientReferenceId;
    }

    private static bool IsSubscriptionActive(string subscriptionStatus)
    {
        return subscriptionStatus.Equals("active", StringComparison.OrdinalIgnoreCase) ||
               subscriptionStatus.Equals("trialing", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsAllowedRedirectUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var redirectUri))
            return false;

        var allowedOrigins = configuration.GetValue<string>("Allowed_Origins")
            ?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (allowedOrigins == null || allowedOrigins.Length == 0)
            return false;

        return allowedOrigins.Any(origin =>
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
                return false;

            return originUri.Scheme.Equals(redirectUri.Scheme, StringComparison.OrdinalIgnoreCase) &&
                   originUri.Host.Equals(redirectUri.Host, StringComparison.OrdinalIgnoreCase) &&
                   originUri.Port == redirectUri.Port;
        });
    }

    public async Task GrantSubscriptionToUser(GrantSubscriptionRequest request)
    {
        var user = await userProfileRepository.GetUserProfileById(request.UserId);
        var customerService = new CustomerService();
        var customer = await customerService.CreateAsync(
            new CustomerCreateOptions
            {
                Email = user.Email,
                Name = user.FirstName + " " + user.LastName
            });
        if (customer == null)
            throw new NullReferenceException("Customer creation failed");
        var subscriptionService = new SubscriptionService();
        var subscription = await subscriptionService.CreateAsync(
            new SubscriptionCreateOptions
            {
                Customer = customer.Id,
                TrialEnd = DateTime.UtcNow.AddYears(1),
                TrialFromPlan = true,
                TrialSettings =
                {
                    EndBehavior =
                    {
                        MissingPaymentMethod = "cancel"
                    }
                },
                Items =
                [
                    new SubscriptionItemOptions
                    {
                        Price = request.PriceId
                    }
                ],
                Metadata = new Dictionary<string, string>
                {
                    { "userId", request.UserId }
                }
            });
        if (subscription == null)
            throw new NullReferenceException("Subscription creation failed");
    }
}