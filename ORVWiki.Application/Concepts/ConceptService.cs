using ORVWiki.Application.Common;
using ORVWiki.Application.Concepts.Dtos;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.Concepts;

public class ConceptService(
    IPagedEntityRepository<Concept> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<Concept, ConceptDto, ConceptListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Concept";

    protected override ConceptDto ToDto(Concept c, int currentChapter) => new(
        c.Id,
        c.PageId,
        c.Page.Slug,
        Spoilers.RenderInline(c.Page.Title, currentChapter),
        c.Page.DiscoveryChapter,
        Spoilers.RenderInline(c.Page.ShortDescription, currentChapter),
        c.Name,
        Spoilers.RenderInline(c.Definition, currentChapter),
        c.ImpactLevel);

    protected override ConceptListItemDto ToListItem(Concept c) => new(
        c.Id,
        c.Page.Slug,
        c.Name,
        c.ImpactLevel,
        c.Page.DiscoveryChapter);
}
