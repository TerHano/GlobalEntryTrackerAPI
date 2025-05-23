using System.Security.Claims;
using Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace GlobalEntryTrackerAPI.Util;

public static class AuthUtil
{
    public static async Task OnJWTValidate(TokenValidatedContext context)
    {
        var externalUserId =
            context.Principal?.FindFirstValue(ClaimTypes
                .NameIdentifier); // Or other user identifier claim
        if (string.IsNullOrEmpty(externalUserId))
        {
            context.Fail("User ID not found in claims");
            return;
        }

        await using var dbContext =
            context.HttpContext.RequestServices
                .GetRequiredService<GlobalEntryTrackerDbContext>();

        var user = await dbContext.Users.FirstOrDefaultAsync(user =>
            user.ExternalId.Equals(externalUserId));
        if (user == null)
        {
            context.Fail("User not found");
            return;
        }

        var userRole = await dbContext.UserRoles.Include(userRoleEntity => userRoleEntity.Role)
            .FirstOrDefaultAsync(u => u.UserId == user.Id);
        var claims = new List<Claim>
        {
            new("InternalId", user.Id.ToString())
        };
        if (userRole != null) claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
        var identity = new ClaimsIdentity(claims);
        context.Principal?.AddIdentity(identity);
    }
}