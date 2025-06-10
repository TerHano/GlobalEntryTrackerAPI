using System.Security.Claims;
using Business.Dto;
using Database;
using GlobalEntryTrackerAPI.Enum;
using GlobalEntryTrackerAPI.Extensions;
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
            new(CustomClaimTypes.InternalId, user.Id.ToString()),
            new(ClaimTypes.Role, user.UserRole.Role.Name)
        };
        var identity = new ClaimsIdentity(claims);
        context.Principal?.AddIdentity(identity);
    }

    public static void SetResponseAuthCookies(
        HttpResponse response,
        AuthToken token)
    {
        response.Cookies.Append(AuthCookie.AccessTokenName, token.AccessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Domain = token.Domain,
                Expires = DateTimeOffset.UtcNow.AddMinutes(90)
            });
        response.Cookies.Append(AuthCookie.RefreshTokenName, token.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Domain = token.Domain,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
    }

    public static void ClearResponseAuthCookies(
        HttpResponse response, string domain)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Domain = domain
        };
        response.Cookies.Delete(AuthCookie.AccessTokenName, cookieOptions);
        response.Cookies.Delete(AuthCookie.RefreshTokenName, cookieOptions);
    }
}