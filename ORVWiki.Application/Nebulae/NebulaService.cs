using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Nebulae.Dtos;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.Nebulae;

public class NebulaService(
    IPagedEntityRepository<Nebula> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<Nebula, NebulaDto, NebulaListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Nebula";

    protected override NebulaDto ToDto(Nebula n, int currentChapter) => new(
        n.Id,
        n.PageId,
        n.Page.Slug,
        Spoilers.RenderInline(n.Page.Title, currentChapter),
        n.Page.DiscoveryChapter,
        Spoilers.RenderInline(n.Page.ShortDescription, currentChapter),
        n.Name,
        n.FounderConstellationId,
        Spoilers.RenderInline(n.Description, currentChapter),
        n.PowerRank);

    protected override NebulaListItemDto ToListItem(Nebula n) => new(
        n.Id,
        n.Page.Slug,
        n.Name,
        n.PowerRank,
        n.Page.DiscoveryChapter);
}
