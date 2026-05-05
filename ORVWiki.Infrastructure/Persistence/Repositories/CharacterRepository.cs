using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Characters;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;

namespace ORVWiki.Infrastructure.Persistence.Repositories;

public class CharacterRepository(AppDbContext db) : Repository<Character>(db), ICharacterRepository
{
    public Task<Character?> GetWithPageByIdAsync(long id, CancellationToken ct = default)
        => Db.Characters
            .Include(c => c.Page)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Character?> GetVisibleBySlugAsync(string slug, int currentChapter, CancellationToken ct = default)
        => Db.Characters
            .AsNoTracking()
            .Include(c => c.Page)
            .FirstOrDefaultAsync(
                c => c.Page.Slug == slug && c.Page.DiscoveryChapter <= currentChapter,
                ct);

    public Task<Character?> GetVisibleByIdAsync(long id, int currentChapter, CancellationToken ct = default)
        => Db.Characters
            .AsNoTracking()
            .Include(c => c.Page)
            .FirstOrDefaultAsync(
                c => c.Id == id && c.Page.DiscoveryChapter <= currentChapter,
                ct);

    public async Task<PaginatedResult<Character>> ListVisibleAsync(
        int currentChapter,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = Db.Characters
            .AsNoTracking()
            .Include(c => c.Page)
            .Where(c => c.Page.DiscoveryChapter <= currentChapter);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.FullName)
            .Skip(pagination.Skip)
            .Take(pagination.SafePageSize)
            .ToListAsync(ct);

        return new PaginatedResult<Character>(items, total, pagination.SafePage, pagination.SafePageSize);
    }

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
        => Db.Pages.AnyAsync(p => p.Slug == slug, ct);
}
