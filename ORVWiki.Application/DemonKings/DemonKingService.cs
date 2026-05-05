using ORVWiki.Application.Common;
using ORVWiki.Application.DemonKings.Dtos;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.DemonKings;

public class DemonKingService(
    IPagedEntityRepository<DemonKing> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<DemonKing, DemonKingDto, DemonKingListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Demon King";

    protected override DemonKingDto ToDto(DemonKing d, int currentChapter) => new(
        d.Id,
        d.PageId,
        d.Page.Slug,
        Spoilers.RenderInline(d.Page.Title, currentChapter),
        d.Page.DiscoveryChapter,
        Spoilers.RenderInline(d.Page.ShortDescription, currentChapter),
        d.Ranking,
        d.Name,
        d.DemonRealm,
        Spoilers.RenderInline(d.Description, currentChapter));

    protected override DemonKingListItemDto ToListItem(DemonKing d) => new(
        d.Id,
        d.Page.Slug,
        d.Ranking,
        d.Name,
        d.Page.DiscoveryChapter);
}
