using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Bookmarks;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;

namespace ORVWiki.Infrastructure.Persistence.Repositories;

public class BookmarkRepository(AppDbContext db)
    : Repository<Bookmark>(db), IBookmarkRepository
{
    public Task<Bookmark?> GetByUserAndPageAsync(long userId, long pageId, CancellationToken ct = default)
        => Db.Bookmarks.FirstOrDefaultAsync(b => b.UserId == userId && b.PageId == pageId, ct);

    public async Task<PaginatedResult<Bookmark>> ListMineAsync(
        long userId, PaginationParams p, CancellationToken ct = default)
    {
        var query = Db.Bookmarks
            .AsNoTracking()
            .Include(b => b.Page)
            .Where(b => b.UserId == userId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip(p.Skip)
            .Take(p.SafePageSize)
            .ToListAsync(ct);

        return new PaginatedResult<Bookmark>(items, total, p.SafePage, p.SafePageSize);
    }

    public Task<bool> PageExistsAsync(long pageId, CancellationToken ct = default)
        => Db.Pages.AnyAsync(p => p.Id == pageId, ct);
}
