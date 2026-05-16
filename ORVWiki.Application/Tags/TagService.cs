using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Tags.Dtos;

namespace ORVWiki.Application.Tags;

public class TagService(IAppDbContext db) : ITagService
{
    // Tags are a small, unspoilerable lookup set, so the whole list is returned
    // unpaginated and ungated — the spoiler gate applies to the pages a tag
    // links to, not to the tag itself.
    public async Task<IReadOnlyList<TagDto>> ListAllAsync(CancellationToken ct = default)
        => await db.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(t.Id, t.Name, t.Slug, t.Color))
            .ToListAsync(ct);
}
