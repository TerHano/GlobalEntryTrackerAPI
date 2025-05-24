using System.Security.Claims;
using Business.Dto;
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

        var user = await dbContext.Users.Include(x => x.UserRole)
            .ThenInclude(userRoleEntity => userRoleEntity.Role).FirstOrDefaultAsync(user =>
                user.ExternalId.Equals(externalUserId));
        if (user == null)
        {
            context.Fail("User not found");
            return;
        }

        var claims = new List<Claim>
        {
            new("InternalId", user.Id.ToString()),
            new(ClaimTypes.Role, user.UserRole.Role.Name)
        };
        var identity = new ClaimsIdentity(claims);
        context.Principal?.AddIdentity(identity);
    }

    public static void SetResponseAuthCookies(
        HttpResponse response,
        AuthToken token)
    {
        response.Cookies.Append("access_token", token.AccessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddMinutes(90)
            });
        response.Cookies.Append("refresh_token", token.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
    }

    public static void ClearResponseAuthCookies(
        HttpResponse response)
    {
        response.Cookies.Delete("access_token");
        response.Cookies.Delete("refresh_token");
    }
}