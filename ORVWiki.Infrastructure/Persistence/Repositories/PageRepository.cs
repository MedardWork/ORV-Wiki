using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;
using ORVWiki.Application.Pages;

namespace ORVWiki.Infrastructure.Persistence.Repositories;

public class PageRepository(AppDbContext db) : Repository<Page>(db), IPageRepository
{
    public Task<Page?> GetVisibleBySlugAsync(string slug, int currentChapter, CancellationToken ct = default)
        => Db.Pages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug && p.DiscoveryChapter <= currentChapter, ct);

    public Task<Page?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => Db.Pages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);

    public async Task<PaginatedResult<Page>> ListVisibleAsync(
        int currentChapter,
        EntityType? entityType,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = Db.Pages
            .AsNoTracking()
            .Where(p => p.DiscoveryChapter <= currentChapter);

        if (entityType.HasValue)
            query = query.Where(p => p.EntityType == entityType.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Title)
            .Skip(pagination.Skip)
            .Take(pagination.SafePageSize)
            .ToListAsync(ct);

        return new PaginatedResult<Page>(items, total, pagination.SafePage, pagination.SafePageSize);
    }
}
