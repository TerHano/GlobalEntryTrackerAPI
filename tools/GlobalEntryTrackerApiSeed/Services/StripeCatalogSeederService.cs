using Database;
using Database.Entities;
using GlobalEntryTrackerApiSeed.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;

namespace GlobalEntryTrackerApiSeed.Services;

public class StripeCatalogSeederService(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    ILogger<StripeCatalogSeederService> logger)
{
    public async Task<StripeCatalogSeedResult> SeedStripeCatalogAsync(
        StripeCatalogSeedOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.StripeSecretKey))
        {
            logger.LogError("STRIPE_SECRET_KEY is required when running --seed-stripe-catalog");
            return new StripeCatalogSeedResult
            {
                Success = false,
                DryRun = options.DryRun,
                Error = "STRIPE_SECRET_KEY is required when running --seed-stripe-catalog"
            };
        }

        try
        {
            logger.LogInformation("Starting Stripe catalog seeding process{Mode}",
                options.DryRun ? " (dry-run)" : string.Empty);
            StripeConfiguration.ApiKey = options.StripeSecretKey;

            var prices = await GetRecurringPrices(options, cancellationToken);
            if (prices.Count == 0)
            {
                logger.LogWarning("No Stripe recurring prices matched the provided filters");
                return new StripeCatalogSeedResult
                {
                    Success = true,
                    DryRun = options.DryRun,
                    MatchedRecurringPrices = 0
                };
            }

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var productService = new ProductService();
            var productCache = new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase);

            var created = 0;
            var updated = 0;
            var skippedMissingIdentifiers = 0;

            foreach (var price in prices)
            {
                if (string.IsNullOrWhiteSpace(price.Id) || string.IsNullOrWhiteSpace(price.ProductId))
                {
                    logger.LogWarning("Skipping price with missing required identifiers");
                    skippedMissingIdentifiers++;
                    continue;
                }

                var product = await GetProduct(
                    productService,
                    productCache,
                    price.ProductId,
                    cancellationToken);
                if (product == null)
                {
                    logger.LogWarning("Skipping price {PriceId}, product {ProductId} not found",
                        price.Id,
                        price.ProductId);
                    continue;
                }

                var name = BuildName(price, product);
                var description = BuildDescription(price, product);
                var features = BuildFeatures(product);

                var existingPlan = await context.PlanOptions
                    .FirstOrDefaultAsync(x => x.PriceId == price.Id, cancellationToken);

                if (existingPlan == null)
                {
                    context.PlanOptions.Add(new PlanOptionEntity
                    {
                        Name = name,
                        Description = description,
                        PriceId = price.Id,
                        Features = features
                    });
                    created++;
                }
                else
                {
                    existingPlan.Name = name;
                    existingPlan.Description = description;
                    existingPlan.Features = features;
                    updated++;
                }
            }

            if (!options.DryRun)
            {
                await context.SaveChangesAsync(cancellationToken);
            }

            logger.LogInformation(
                "Stripe catalog seeding completed. Created: {Created}, Updated: {Updated}{DryRunSuffix}",
                created,
                updated,
                options.DryRun ? " (no database changes were persisted)" : string.Empty);
            return new StripeCatalogSeedResult
            {
                Success = true,
                DryRun = options.DryRun,
                MatchedRecurringPrices = prices.Count,
                Created = created,
                Updated = updated,
                SkippedMissingIdentifiers = skippedMissingIdentifiers
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding Stripe catalog");
            return new StripeCatalogSeedResult
            {
                Success = false,
                DryRun = options.DryRun,
                Error = ex.Message
            };
        }
    }

    private async Task<List<Price>> GetRecurringPrices(
        StripeCatalogSeedOptions options,
        CancellationToken cancellationToken)
    {
        var priceService = new PriceService();

        if (options.PriceIds.Count > 0)
        {
            var pricesById = new List<Price>();
            foreach (var priceId in options.PriceIds)
            {
                var price = await priceService.GetAsync(priceId, null, null, cancellationToken);
                if (price.Recurring == null)
                {
                    continue;
                }

                if (options.OnlyActive && !price.Active)
                {
                    continue;
                }

                if (options.ProductIds.Count > 0 &&
                    !string.IsNullOrWhiteSpace(price.ProductId) &&
                    !options.ProductIds.Contains(price.ProductId))
                {
                    continue;
                }

                pricesById.Add(price);
            }

            return pricesById;
        }

        var prices = new List<Price>();
        string? startingAfter = null;

        do
        {
            var listOptions = new PriceListOptions
            {
                Limit = 100,
                Type = "recurring",
                StartingAfter = startingAfter
            };

            if (options.OnlyActive)
            {
                listOptions.Active = true;
            }

            var pricePage = await priceService.ListAsync(listOptions, null, cancellationToken);
            if (pricePage.Data.Count == 0)
            {
                break;
            }

            foreach (var price in pricePage.Data)
            {
                if (options.ProductIds.Count > 0 &&
                    !string.IsNullOrWhiteSpace(price.ProductId) &&
                    !options.ProductIds.Contains(price.ProductId))
                {
                    continue;
                }

                prices.Add(price);
            }

            startingAfter = pricePage.HasMore ? pricePage.Data[^1].Id : null;
        } while (startingAfter != null);

        return prices;
    }

    private static async Task<Product?> GetProduct(
        ProductService productService,
        Dictionary<string, Product> productCache,
        string productId,
        CancellationToken cancellationToken)
    {
        if (productCache.TryGetValue(productId, out var cachedProduct))
        {
            return cachedProduct;
        }

        var product = await productService.GetAsync(productId, null, null, cancellationToken);
        productCache[productId] = product;
        return product;
    }

    private static string BuildName(Price price, Product product)
    {
        var fallbackName = !string.IsNullOrWhiteSpace(price.Nickname)
            ? price.Nickname
            : price.Id;
        var selectedName = !string.IsNullOrWhiteSpace(product.Name)
            ? product.Name
            : fallbackName;
        return Truncate(selectedName, 100);
    }

    private static string BuildDescription(Price price, Product product)
    {
        if (!string.IsNullOrWhiteSpace(product.Description))
        {
            return Truncate(product.Description, 300);
        }

        var interval = price.Recurring?.Interval ?? "month";
        var intervalCount = price.Recurring?.IntervalCount ?? 1;
        var computedDescription = intervalCount == 1
            ? $"Subscription billed every {interval}."
            : $"Subscription billed every {intervalCount} {interval}s.";
        return Truncate(computedDescription, 300);
    }

    private static string[] BuildFeatures(Product product)
    {
        if (product.Metadata.TryGetValue("features", out var metadataFeatures) &&
            !string.IsNullOrWhiteSpace(metadataFeatures))
        {
            var parsedFeatures = metadataFeatures
                .Split([',', '|', '\n'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Truncate(x, 120))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (parsedFeatures.Length > 0)
            {
                return parsedFeatures;
            }
        }

        return ["Subscription access"];
    }

    private static string Truncate(string value, int maxLength)
    {
        var trimmed = value.Trim();
        if (trimmed.Length <= maxLength)
        {
            return trimmed;
        }

        return trimmed[..maxLength];
    }
}