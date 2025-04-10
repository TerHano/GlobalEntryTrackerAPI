using System.Security.Claims;

namespace GlobalEntryTrackerAPI.Extensions;

public static class ClaimsPrincipalExtension
{
    public static int GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var userId = claimsPrincipal.FindFirstValue("InternalId");
        if (userId == null) throw new Exception("No user id found");
        return int.Parse(userId);
    }
}