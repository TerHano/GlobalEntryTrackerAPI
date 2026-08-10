using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Database.Entities;

public class RoleEntity(
    string name,
    string code,
    int maxTrackers,
    int notificationIntervalInMinutes,
    int notificationDelayInMinutes)
    : IdentityRole(name)
{
    public virtual ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();

    [MaxLength(20)] public string Code { get; set; } = code;

    public int MaxTrackers { get; set; } = maxTrackers;

    /// <summary>
    ///     Cooldown between notifications. After a user is actually notified they will not be
    ///     notified again until this many minutes have passed. Acts as an anti-spam cap.
    /// </summary>
    public int NotificationIntervalInMinutes { get; set; } = notificationIntervalInMinutes;

    /// <summary>
    ///     How long an appointment must have been visible to us before this role is told about it.
    ///     This is the tier lever: paid roles use 0 (told on the next scan), free roles are held back
    ///     so that subscribers get first look at newly released appointments.
    /// </summary>
    public int NotificationDelayInMinutes { get; set; } = notificationDelayInMinutes;
}