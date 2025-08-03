using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Database.Entities;

public class RoleEntity(
    string name,
    string code,
    int maxTrackers,
    int notificationIntervalInMinutes)
    : IdentityRole(name)
{
    public virtual ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();

    [MaxLength(20)] public string Code { get; set; } = code;

    public int MaxTrackers { get; set; } = maxTrackers;
    public int NotificationIntervalInMinutes { get; set; } = notificationIntervalInMinutes;
}