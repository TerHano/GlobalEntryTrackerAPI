namespace Database.Entities;

public class RoleEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int MaxTrackers { get; set; }
    public int NotificationIntervalInMinutes { get; set; }
}