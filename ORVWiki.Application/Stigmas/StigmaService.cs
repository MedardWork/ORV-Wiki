using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Spoilers;
using ORVWiki.Application.Stigmas.Dtos;

namespace ORVWiki.Application.Stigmas;

public class StigmaService(
    IPagedEntityRepository<Stigma> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<Stigma, StigmaDto, StigmaListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Stigma";

    protected override StigmaDto ToDto(Stigma s, int currentChapter) => new(
        s.Id,
        s.PageId,
        s.Page.Slug,
        Spoilers.RenderInline(s.Page.Title, currentChapter),
        s.Page.DiscoveryChapter,
        Spoilers.RenderInline(s.Page.ShortDescription, currentChapter),
        s.Name,
        s.ProviderConstellationId,
        s.ActivationCost,
        Spoilers.RenderInline(s.Effect, currentChapter));

    protected override StigmaListItemDto ToListItem(Stigma s) => new(
        s.Id,
        s.Page.Slug,
        s.Name,
        s.ProviderConstellationId,
        s.Page.DiscoveryChapter);
}
