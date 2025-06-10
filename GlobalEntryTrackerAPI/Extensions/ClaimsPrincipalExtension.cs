using System.Security.Claims;

namespace GlobalEntryTrackerAPI.Extensions;

public static class ClaimsPrincipalExtension
{
    public static int GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var userId = claimsPrincipal.FindFirstValue(CustomClaimTypes.InternalId);
        if (userId == null) throw new Exception("No user id found");
        return int.Parse(userId);
    }

    public static string GetBearerToken(this HttpRequest request)
    {
        var authHeader = request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            throw new Exception("No bearer token found");

        var jwt = authHeader["Bearer ".Length..];
        return jwt;
    }
}