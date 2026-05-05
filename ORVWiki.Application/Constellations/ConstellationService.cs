using ORVWiki.Application.Common;
using ORVWiki.Application.Constellations.Dtos;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.Constellations;

public class ConstellationService(
    IPagedEntityRepository<Constellation> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<Constellation, ConstellationDto, ConstellationListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Constellation";

    protected override ConstellationDto ToDto(Constellation c, int currentChapter) => new(
        c.Id,
        c.PageId,
        c.Page.Slug,
        Spoilers.RenderInline(c.Page.Title, currentChapter),
        c.Page.DiscoveryChapter,
        Spoilers.RenderInline(c.Page.ShortDescription, currentChapter),
        c.Modifier,
        c.TrueName,
        c.NebulaId,
        c.Grade,
        c.OriginCharacterId,
        Spoilers.RenderInline(c.Description, currentChapter));

    protected override ConstellationListItemDto ToListItem(Constellation c) => new(
        c.Id,
        c.Page.Slug,
        c.Modifier,
        c.TrueName,
        c.Grade,
        c.Page.DiscoveryChapter);
}
