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
    UserRoleRepository userRoleRepository,
    UserProfileRepository userProfileRepository,
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
        if (request.PriceId == null) throw new Exception("Price Id is required");
        var options = new SessionCreateOptions
        {
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = request.PriceId,
                    Quantity = 1
                }
            ],

            Mode = "subscription",
            AllowPromotionCodes = true,
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
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
            configuration["Stripe:Webhook_Secret"] ?? null;

        //Get request body
        try
        {
            var stripeEvent =
                EventUtility.ConstructEvent(jsonBody, stripeSignature, stripeWebhookSecret);
            switch (stripeEvent.Type)
            {
                case EventTypes.CustomerSubscriptionCreated:
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
                        throw new NullReferenceException(
                            "UserId not found in subscription metadata");
                    }


                    var userCustomer = new UserCustomerEntity
                    {
                        UserId = userId,
                        SubscriptionId = subscriptionEvent.Id,
                        CustomerId = subscriptionEvent.CustomerId
                    };
                    await userRoleRepository.AddEditRoleForUser(userId, Role.Subscriber);
                    await userCustomerRepository.AddEditUserCustomer(userCustomer);

                    //Move up users next notification date
                    var user = await userProfileRepository.GetUserProfileById(userId);
                    if (user == null)
                    {
                        logger.LogError("User not found");
                        throw new NullReferenceException("User not found");
                    }

                    var roles = roleManager.Roles.ToList();
                    userRoleService.UpdateNextNotificationTimeForUser(user, roles);
                    await userProfileRepository.UpdateUserProfile(user);
                    break;
                }
                case EventTypes.CustomerSubscriptionDeleted:
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
                        throw new NullReferenceException(
                            "UserId not found in subscription metadata");
                    }

                    var userIdInt = int.Parse(userId);
                    await userRoleRepository.RemoveRoleForUser(userId, Role.Subscriber);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            throw new ApplicationException(
                "Error processing subscription event, please try again later");
        }
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