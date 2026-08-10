namespace Database.Entities;

/// <summary>
///     Records the first time a specific appointment slot was observed at a location.
///     This is what the per-role notification delay is measured from, so that "how long has this
///     been available" is a property of the appointment rather than of whoever happens to be due.
/// </summary>
public class SeenAppointmentEntity
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public AppointmentLocationEntity Location { get; set; } = null!;

    /// <summary>Start time of the appointment slot, normalised via <see cref="NormalizeStart" />.</summary>
    public DateTime StartTimestamp { get; set; }

    public DateTime FirstSeenAt { get; set; }

    /// <summary>
    ///     Normalises a slot start time to the representation used as the lookup key.
    ///     Matches the convention already used for
    ///     <see cref="TrackedLocationForUserEntity.LastSeenEarliestAppointment" /> so the two agree.
    /// </summary>
    public static DateTime NormalizeStart(DateTime startTimestamp)
    {
        return startTimestamp.ToUniversalTime();
    }
}
