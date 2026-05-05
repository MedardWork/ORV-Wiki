using ORVWiki.Application.Bookmarks.Dtos;
using ORVWiki.Application.Common;

namespace ORVWiki.Application.Bookmarks;

public interface IBookmarkService
{
    /// <summary>
    /// Returns true if the bookmark was added, false if it was removed.
    /// </summary>
    Task<bool> ToggleAsync(long userId, long pageId, CancellationToken ct = default);

    Task<PaginatedResult<BookmarkDto>> ListMineAsync(long userId, PaginationParams p, CancellationToken ct = default);
}
