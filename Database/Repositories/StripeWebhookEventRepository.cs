using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class StripeWebhookEventRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    ILogger<StripeWebhookEventRepository> logger)
{
    public async Task<bool> TryStartProcessing(string eventId, string eventType)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var entity = new StripeWebhookEventEntity
            {
                EventId = eventId,
                EventType = eventType,
                IsProcessed = false,
                CreatedAtUtc = DateTime.UtcNow,
                ProcessedAtUtc = null
            };
            await context.StripeWebhookEvents.AddAsync(entity);
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task MarkProcessed(string eventId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var webhookEvent = await context.StripeWebhookEvents
            .FirstOrDefaultAsync(x => x.EventId == eventId);
        if (webhookEvent == null)
            return;

        webhookEvent.IsProcessed = true;
        webhookEvent.ProcessedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task ReleaseProcessingLock(string eventId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var webhookEvent = await context.StripeWebhookEvents
            .FirstOrDefaultAsync(x => x.EventId == eventId && !x.IsProcessed);
        if (webhookEvent == null)
            return;

        context.StripeWebhookEvents.Remove(webhookEvent);
        await context.SaveChangesAsync();
        logger.LogInformation("Released Stripe webhook processing lock for event {StripeEventId}", eventId);
    }
}