using Database;
using Database.Entities;
using GlobalEntryTrackerApiSeed.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;

namespace GlobalEntryTrackerApiSeed.Services;

public class StripeSubscriberBackfillService(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    ILogger<StripeSubscriberBackfillService> logger)
{
    public async Task<StripeSubscriberBackfillResult> BackfillSubscribersAsync(
        StripeSubscriberBackfillOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.StripeSecretKey))
        {
            logger.LogError(
                "STRIPE_SECRET_KEY is required when running --backfill-stripe-subscribers");
            return new StripeSubscriberBackfillResult
            {
                Success = false,
                DryRun = options.DryRun,
                Error = "STRIPE_SECRET_KEY is required when running --backfill-stripe-subscribers"
            };
        }

        try
        {
            logger.LogInformation("Starting Stripe subscriber backfill process{Mode}",
                options.DryRun ? " (dry-run)" : string.Empty);
            StripeConfiguration.ApiKey = options.StripeSecretKey;

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var subscriberRoleId = await context.Roles
                .Where(x => x.Name == "Subscriber")
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(subscriberRoleId))
            {
                logger.LogError("Subscriber role does not exist in database");
                return new StripeSubscriberBackfillResult
                {
                    Success = false,
                    DryRun = options.DryRun,
                    Error = "Subscriber role does not exist in database"
                };
            }

            var customerService = new CustomerService();
            var subscriptionService = new SubscriptionService();
            var customerCache = new Dictionary<string, Customer>(StringComparer.OrdinalIgnoreCase);

            var processed = 0;
            var linked = 0;
            var roleAssignments = 0;
            var skippedNoMatch = 0;

            string? startingAfter = null;

            do
            {
                var listOptions = new SubscriptionListOptions
                {
                    Limit = 100,
                    Status = "all",
                    StartingAfter = startingAfter
                };

                var subscriptionPage =
                    await subscriptionService.ListAsync(listOptions, null, cancellationToken);
                if (subscriptionPage.Data.Count == 0)
                {
                    break;
                }

                foreach (var subscription in subscriptionPage.Data)
                {
                    processed++;
                    if (!options.SubscriptionStatuses.Contains(
                            subscription.Status,
                            StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var userId = await ResolveUserId(
                        context,
                        customerService,
                        customerCache,
                        subscription,
                        options.MatchByEmail,
                        cancellationToken);
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        skippedNoMatch++;
                        continue;
                    }

                    await UpsertUserCustomer(
                        context,
                        userId,
                        subscription.CustomerId,
                        subscription.Id,
                        cancellationToken);
                    linked++;

                    var hasSubscriberRole = await context.Set<IdentityUserRole<string>>()
                        .AnyAsync(
                            x => x.UserId == userId && x.RoleId == subscriberRoleId,
                            cancellationToken);
                    if (!hasSubscriberRole)
                    {
                        await context.Set<IdentityUserRole<string>>().AddAsync(
                            new IdentityUserRole<string>
                            {
                                UserId = userId,
                                RoleId = subscriberRoleId
                            },
                            cancellationToken);
                        roleAssignments++;
                    }
                }

                if (!options.DryRun)
                {
                    await context.SaveChangesAsync(cancellationToken);
                }
                startingAfter = subscriptionPage.HasMore ? subscriptionPage.Data[^1].Id : null;
            } while (startingAfter != null);

            logger.LogInformation(
                "Stripe subscriber backfill completed. Processed: {Processed}, Linked: {Linked}, RoleAssignments: {RoleAssignments}, Unmatched: {Unmatched}{DryRunSuffix}",
                processed,
                linked,
                roleAssignments,
                skippedNoMatch,
                options.DryRun ? " (no database changes were persisted)" : string.Empty);
            return new StripeSubscriberBackfillResult
            {
                Success = true,
                DryRun = options.DryRun,
                Processed = processed,
                Linked = linked,
                RoleAssignments = roleAssignments,
                Unmatched = skippedNoMatch
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while backfilling Stripe subscribers");
            return new StripeSubscriberBackfillResult
            {
                Success = false,
                DryRun = options.DryRun,
                Error = ex.Message
            };
        }
    }

    private static async Task UpsertUserCustomer(
        GlobalEntryTrackerDbContext context,
        string userId,
        string customerId,
        string subscriptionId,
        CancellationToken cancellationToken)
    {
        var existing = await context.UserCustomers
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (existing == null)
        {
            await context.UserCustomers.AddAsync(new UserCustomerEntity
            {
                UserId = userId,
                CustomerId = customerId,
                SubscriptionId = subscriptionId
            }, cancellationToken);
            return;
        }

        existing.CustomerId = customerId;
        existing.SubscriptionId = subscriptionId;
    }

    private static async Task<string?> ResolveUserId(
        GlobalEntryTrackerDbContext context,
        CustomerService customerService,
        Dictionary<string, Customer> customerCache,
        Subscription subscription,
        bool matchByEmail,
        CancellationToken cancellationToken)
    {
        if (subscription.Metadata.TryGetValue("userId", out var userIdFromSubscription) &&
            !string.IsNullOrWhiteSpace(userIdFromSubscription))
        {
            return userIdFromSubscription;
        }

        if (string.IsNullOrWhiteSpace(subscription.CustomerId))
        {
            return null;
        }

        var customer = await GetCustomer(
            customerService,
            customerCache,
            subscription.CustomerId,
            cancellationToken);
        if (customer == null)
        {
            return null;
        }

        if (customer.Metadata.TryGetValue("userId", out var userIdFromCustomer) &&
            !string.IsNullOrWhiteSpace(userIdFromCustomer))
        {
            return userIdFromCustomer;
        }

        if (!matchByEmail || string.IsNullOrWhiteSpace(customer.Email))
        {
            return null;
        }

        var normalizedEmail = customer.Email.Trim().ToLowerInvariant();
        var userProfile = await context.UserProfiles
            .FirstOrDefaultAsync(
                x => x.Email.ToLower() == normalizedEmail,
                cancellationToken);
        return userProfile?.UserId;
    }

    private static async Task<Customer?> GetCustomer(
        CustomerService customerService,
        Dictionary<string, Customer> customerCache,
        string customerId,
        CancellationToken cancellationToken)
    {
        if (customerCache.TryGetValue(customerId, out var cachedCustomer))
        {
            return cachedCustomer;
        }

        var customer = await customerService.GetAsync(customerId, null, null, cancellationToken);
        customerCache[customerId] = customer;
        return customer;
    }
}