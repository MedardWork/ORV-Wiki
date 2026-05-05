using ORVWiki.Application.Common;
using ORVWiki.Application.Dokkaebis.Dtos;
using ORVWiki.Application.Spoilers;
using DokkaebiEntity = ORVWiki.Application.Entities.Dokkaebi;

namespace ORVWiki.Application.Dokkaebis;

public class DokkaebiService(
    IPagedEntityRepository<DokkaebiEntity> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<DokkaebiEntity, DokkaebiDto, DokkaebiListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Dokkaebi";

    protected override DokkaebiDto ToDto(DokkaebiEntity d, int currentChapter) => new(
        d.Id,
        d.PageId,
        d.Page.Slug,
        Spoilers.RenderInline(d.Page.Title, currentChapter),
        d.Page.DiscoveryChapter,
        Spoilers.RenderInline(d.Page.ShortDescription, currentChapter),
        d.Name,
        d.ChannelId,
        d.Rank,
        Spoilers.RenderInline(d.Speciality, currentChapter));

    protected override DokkaebiListItemDto ToListItem(DokkaebiEntity d) => new(
        d.Id,
        d.Page.Slug,
        d.Name,
        d.Rank,
        d.Page.DiscoveryChapter);
}
