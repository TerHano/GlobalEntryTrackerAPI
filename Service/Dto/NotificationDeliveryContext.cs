namespace Service.Dto;

/// <summary>
///     Describes how an alert reached a particular user, so the notification itself can explain
///     any delay that was applied and point at the upgrade that removes it.
/// </summary>
public class NotificationDeliveryContext
{
    public static readonly NotificationDeliveryContext Immediate = new();

    /// <summary>Minutes this user's role held the appointment back before releasing it.</summary>
    public int DelayInMinutes { get; init; }

    /// <summary>Where to send the user to upgrade. Null when not configured.</summary>
    public string? UpgradeUrl { get; init; }

    public bool IsDelayed => DelayInMinutes > 0;
}
