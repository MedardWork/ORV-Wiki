namespace ORVWiki.Application.Pages;

/// <summary>
/// Centralized cache key strategy for Page lookups so writers and readers
/// agree on what to invalidate.
/// </summary>
public static class PageCacheKeys
{
    public static string BySlug(string slug) => $"page:slug:{slug}";
}
