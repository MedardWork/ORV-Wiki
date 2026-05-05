using ORVWiki.Application.Attributes.Dtos;
using ORVWiki.Application.Common;
using ORVWiki.Application.Spoilers;
using AttributeEntity = ORVWiki.Application.Entities.Attribute;

namespace ORVWiki.Application.Attributes;

public class AttributeService(
    IPagedEntityRepository<AttributeEntity> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<AttributeEntity, AttributeDto, AttributeListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Attribute";

    protected override AttributeDto ToDto(AttributeEntity a, int currentChapter) => new(
        a.Id,
        a.PageId,
        a.Page.Slug,
        Spoilers.RenderInline(a.Page.Title, currentChapter),
        a.Page.DiscoveryChapter,
        Spoilers.RenderInline(a.Page.ShortDescription, currentChapter),
        a.Name,
        a.Rarity,
        Spoilers.RenderInline(a.Effect, currentChapter));

    protected override AttributeListItemDto ToListItem(AttributeEntity a) => new(
        a.Id,
        a.Page.Slug,
        a.Name,
        a.Rarity,
        a.Page.DiscoveryChapter);
}
