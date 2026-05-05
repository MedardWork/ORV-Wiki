using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;

namespace ORVWiki.Application.Bookmarks;

public interface IBookmarkRepository : IRepository<Bookmark>
{
    Task<Bookmark?> GetByUserAndPageAsync(long userId, long pageId, CancellationToken ct = default);
    Task<PaginatedResult<Bookmark>> ListMineAsync(long userId, PaginationParams p, CancellationToken ct = default);
    Task<bool> PageExistsAsync(long pageId, CancellationToken ct = default);
}
