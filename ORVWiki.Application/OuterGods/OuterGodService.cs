using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.OuterGods.Dtos;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.OuterGods;

public class OuterGodService(
    IPagedEntityRepository<OuterGod> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<OuterGod, OuterGodDto, OuterGodListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Outer God";

    protected override OuterGodDto ToDto(OuterGod o, int currentChapter) => new(
        o.Id,
        o.PageId,
        o.Page.Slug,
        Spoilers.RenderInline(o.Page.Title, currentChapter),
        o.Page.DiscoveryChapter,
        Spoilers.RenderInline(o.Page.ShortDescription, currentChapter),
        o.Name,
        o.GodType,
        Spoilers.RenderInline(o.Description, currentChapter));

    protected override OuterGodListItemDto ToListItem(OuterGod o) => new(
        o.Id,
        o.Page.Slug,
        o.Name,
        o.GodType,
        o.Page.DiscoveryChapter);
}
