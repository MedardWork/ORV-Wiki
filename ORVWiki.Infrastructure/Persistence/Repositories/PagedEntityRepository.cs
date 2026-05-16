using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;

namespace ORVWiki.Infrastructure.Persistence.Repositories;

public class PagedEntityRepository<TEntity>(AppDbContext db) : IPagedEntityRepository<TEntity>
    where TEntity : class, IPagedEntity
{
    protected IQueryable<TEntity> VisibleQuery(int currentChapter) =>
        db.Set<TEntity>()
            .AsNoTracking()
            .Include(e => e.Page)
            .Where(e => e.Page.DiscoveryChapter <= currentChapter);

    // Backs single-entity reads. The base returns VisibleQuery unchanged;
    // subclasses override to eager-load the relationships their detail page
    // surfaces, while list reads stay on the lean VisibleQuery.
    protected virtual IQueryable<TEntity> DetailQuery(int currentChapter) => VisibleQuery(currentChapter);

    public Task<TEntity?> GetVisibleByIdAsync(long id, int currentChapter, CancellationToken ct = default)
        => DetailQuery(currentChapter).FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<TEntity?> GetVisibleBySlugAsync(string slug, int currentChapter, CancellationToken ct = default)
        => DetailQuery(currentChapter).FirstOrDefaultAsync(e => e.Page.Slug == slug, ct);

    public async Task<PaginatedResult<TEntity>> ListVisibleAsync(
        int currentChapter,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = VisibleQuery(currentChapter);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.Page.Title)
            .Skip(pagination.Skip)
            .Take(pagination.SafePageSize)
            .ToListAsync(ct);

        return new PaginatedResult<TEntity>(items, total, pagination.SafePage, pagination.SafePageSize);
    }
}
