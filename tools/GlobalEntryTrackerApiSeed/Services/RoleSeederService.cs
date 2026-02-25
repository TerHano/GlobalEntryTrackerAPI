using Database.Entities;
using Database.Enums;
using GlobalEntryTrackerApiSeed.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace GlobalEntryTrackerApiSeed.Services;

public class RoleSeederService(
    RoleManager<RoleEntity> roleManager,
    ILogger<RoleSeederService> logger)
{
    public async Task<RoleSeedResult> SeedRolesAsync(bool dryRun)
    {
        logger.LogInformation("Starting role seeding (dry-run={DryRun})", dryRun);

        try
        {
            var rolesToSeed = GetRoles();
            var addedCount = 0;
            var skippedCount = 0;
            var addedRoles = new List<string>();

            foreach (var role in rolesToSeed)
            {
                var exists = await roleManager.RoleExistsAsync(role.Name!);

                if (exists)
                {
                    logger.LogInformation("Role '{Name}' already exists, skipping", role.Name);
                    skippedCount++;
                }
                else
                {
                    logger.LogInformation("Adding role '{Name}'", role.Name);

                    if (!dryRun)
                    {
                        var result = await roleManager.CreateAsync(role);
                        if (!result.Succeeded)
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            logger.LogError("Failed to create role '{Name}': {Errors}", role.Name, errors);
                            return new RoleSeedResult
                            {
                                Success = false,
                                ErrorMessage = $"Failed to create role '{role.Name}': {errors}",
                                Added = addedCount,
                                Skipped = skippedCount,
                                AddedRoles = addedRoles
                            };
                        }
                    }

                    addedCount++;
                    addedRoles.Add(role.Name!);
                }
            }

            logger.LogInformation("Role seeding completed: Added={Added}, Skipped={Skipped}",
                addedCount, skippedCount);

            return new RoleSeedResult
            {
                Success = true,
                Added = addedCount,
                Skipped = skippedCount,
                AddedRoles = addedRoles
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding roles");
            return new RoleSeedResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static List<RoleEntity> GetRoles()
    {
        return
        [
            new RoleEntity(nameof(Role.Free), Role.Free.GetCode(), 2, 300),
            new RoleEntity(nameof(Role.Subscriber), Role.Subscriber.GetCode(), 5, 1000),
            new RoleEntity(nameof(Role.Admin), Role.Admin.GetCode(), 10, 1),
            new RoleEntity(nameof(Role.FriendsFamily), Role.FriendsFamily.GetCode(), 10, 1)
        ];
    }
}

