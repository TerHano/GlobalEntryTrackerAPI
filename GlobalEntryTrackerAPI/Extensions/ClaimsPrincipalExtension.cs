using System.Security.Claims;

namespace GlobalEntryTrackerAPI.Extensions;

public static class ClaimsPrincipalExtension
{
    public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) throw new UnauthorizedAccessException("No user id found in claims");
        return userId;
    }

    public static string GetBearerToken(this HttpRequest request)
    {
        var authHeader = request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            throw new UnauthorizedAccessException("No bearer token found in authorization header");

        var jwt = authHeader["Bearer ".Length..];
        return jwt;
    }
}