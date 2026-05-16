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
        => DetailQuery(currentChapter).FirstOrDefaultAsync(c => c.Page.Slug == slug, ct);

    public Task<Character?> GetVisibleByIdAsync(long id, int currentChapter, CancellationToken ct = default)
        => DetailQuery(currentChapter).FirstOrDefaultAsync(c => c.Id == id, ct);

    // A visible character plus every relationship its wiki page surfaces. Each
    // relation collection is filtered to targets the reader has already
    // discovered, so the spoiler gate holds for linked entities too — a reader
    // never learns a character has a skill/stigma whose page is still hidden.
    // Split query: ten collection includes in one statement would multiply into
    // a cartesian blow-up.
    private IQueryable<Character> DetailQuery(int currentChapter)
        => Db.Characters
            .AsNoTracking()
            .AsSplitQuery()
            .Where(c => c.Page.DiscoveryChapter <= currentChapter)
            .Include(c => c.Page).ThenInclude(p => p.PageTags).ThenInclude(pt => pt.Tag)
            .Include(c => c.CharacterStigmas
                    .Where(x => x.Stigma.Page.DiscoveryChapter <= currentChapter))
                .ThenInclude(x => x.Stigma).ThenInclude(s => s.Page)
            .Include(c => c.CharacterAttributes
                    .Where(x => x.Attribute.Page.DiscoveryChapter <= currentChapter))
                .ThenInclude(x => x.Attribute).ThenInclude(a => a.Page)
            .Include(c => c.CharacterSkills
                    .Where(x => x.Skill.Page.DiscoveryChapter <= currentChapter))
                .ThenInclude(x => x.Skill).ThenInclude(s => s.Page)
            .Include(c => c.CharacterItems
                    .Where(x => x.Item.Page.DiscoveryChapter <= currentChapter))
                .ThenInclude(x => x.Item).ThenInclude(i => i.Page)
            .Include(c => c.CharacterFables
                    .Where(x => x.Fable.Page.DiscoveryChapter <= currentChapter))
                .ThenInclude(x => x.Fable).ThenInclude(f => f.Page)
            .Include(c => c.CharacterConstellations
                    .Where(x => x.Constellation.Page.DiscoveryChapter <= currentChapter))
                .ThenInclude(x => x.Constellation).ThenInclude(co => co.Page)
            .Include(c => c.EventCharacters
                    .Where(x => x.Event.Page.DiscoveryChapter <= currentChapter))
                .ThenInclude(x => x.Event).ThenInclude(ev => ev.Page)
            .Include(c => c.ScenarioParticipants
                    .Where(x => x.Scenario.Page.DiscoveryChapter <= currentChapter))
                .ThenInclude(x => x.Scenario).ThenInclude(sc => sc.Page)
            .Include(c => c.DeifiedConstellations
                    .Where(co => co.Page.DiscoveryChapter <= currentChapter))
                .ThenInclude(co => co.Page)
            .Include(c => c.OriginatedFables
                    .Where(f => f.Page.DiscoveryChapter <= currentChapter))
                .ThenInclude(f => f.Page);

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
