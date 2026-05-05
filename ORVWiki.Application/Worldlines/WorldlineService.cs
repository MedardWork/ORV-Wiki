using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Spoilers;
using ORVWiki.Application.Worldlines.Dtos;

namespace ORVWiki.Application.Worldlines;

public class WorldlineService(
    IPagedEntityRepository<Worldline> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<Worldline, WorldlineDto, WorldlineListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Worldline";

    protected override WorldlineDto ToDto(Worldline w, int currentChapter) => new(
        w.Id,
        w.PageId,
        w.Page.Slug,
        Spoilers.RenderInline(w.Page.Title, currentChapter),
        w.Page.DiscoveryChapter,
        Spoilers.RenderInline(w.Page.ShortDescription, currentChapter),
        w.LineNumber,
        w.Name,
        w.ParentWorldlineId,
        w.IsMain,
        Spoilers.RenderInline(w.Description, currentChapter));

    protected override WorldlineListItemDto ToListItem(Worldline w) => new(
        w.Id,
        w.Page.Slug,
        w.LineNumber,
        w.Name,
        w.IsMain,
        w.Page.DiscoveryChapter);
}
