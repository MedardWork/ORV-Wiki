using ORVWiki.Application.Arcs.Dtos;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.Arcs;

public class ArcService(
    IPagedEntityRepository<Arc> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<Arc, ArcDto, ArcListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Arc";

    protected override ArcDto ToDto(Arc a, int currentChapter) => new(
        a.Id,
        a.PageId,
        a.Page.Slug,
        Spoilers.RenderInline(a.Page.Title, currentChapter),
        a.Page.DiscoveryChapter,
        Spoilers.RenderInline(a.Page.ShortDescription, currentChapter),
        a.Name,
        a.OrderNumber,
        a.ChapterStart,
        a.ChapterEnd,
        Spoilers.RenderInline(a.Summary, currentChapter));

    protected override ArcListItemDto ToListItem(Arc a) => new(
        a.Id,
        a.Page.Slug,
        a.Name,
        a.OrderNumber,
        a.ChapterStart,
        a.ChapterEnd,
        a.Page.DiscoveryChapter);
}
