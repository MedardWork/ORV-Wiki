using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Pages;

public interface IPageRepository : IRepository<Page>
{
    /// <summary>
    /// Returns the page only if its <c>discovery_chapter</c> is at or below
    /// the caller's current chapter (spoiler gate). Returns null otherwise.
    /// </summary>
    Task<Page?> GetVisibleBySlugAsync(string slug, int currentChapter, CancellationToken ct = default);

    /// <summary>
    /// Slug lookup that ignores the spoiler gate — used by the cache layer so
    /// the cached row can be reused across users at any current_chapter; the
    /// caller (service) applies the gate after the cache lookup.
    /// </summary>
    Task<Page?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<PaginatedResult<Page>> ListVisibleAsync(
        int currentChapter,
        EntityType? entityType,
        PaginationParams pagination,
        CancellationToken ct = default);
}
