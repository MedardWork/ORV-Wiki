using System.Security.Claims;

namespace ORVWiki.API.Auth;

public static class CurrentUser
{
    public const string CurrentChapterClaim = "current_chapter";

    public static long GetId(ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Token is missing NameIdentifier claim.");
        return long.Parse(raw);
    }

    /// <summary>
    /// Returns the caller's current_chapter claim, or 0 (sees nothing) if missing/anonymous.
    /// </summary>
    public static int GetCurrentChapter(ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(CurrentChapterClaim);
        return int.TryParse(raw, out var v) ? v : 0;
    }
}
