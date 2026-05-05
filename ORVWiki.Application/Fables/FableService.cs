using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Fables.Dtos;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.Fables;

public class FableService(
    IPagedEntityRepository<Fable> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<Fable, FableDto, FableListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Fable";

    protected override FableDto ToDto(Fable f, int currentChapter) => new(
        f.Id,
        f.PageId,
        f.Page.Slug,
        Spoilers.RenderInline(f.Page.Title, currentChapter),
        f.Page.DiscoveryChapter,
        Spoilers.RenderInline(f.Page.ShortDescription, currentChapter),
        Spoilers.RenderInline(f.Title, currentChapter),
        f.Grade,
        Spoilers.RenderInline(f.Legend, currentChapter),
        f.OriginCharacterId);

    protected override FableListItemDto ToListItem(Fable f) => new(
        f.Id,
        f.Page.Slug,
        f.Title,
        f.Grade,
        f.Page.DiscoveryChapter);
}
