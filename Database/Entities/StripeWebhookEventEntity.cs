using System.ComponentModel.DataAnnotations;

namespace Database.Entities;

public class StripeWebhookEventEntity
{
    public int Id { get; set; }

    [MaxLength(255)] public required string EventId { get; set; }

    [MaxLength(255)] public required string EventType { get; set; }

    public bool IsProcessed { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }
}