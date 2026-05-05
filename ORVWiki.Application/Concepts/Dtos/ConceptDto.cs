using ORVWiki.Application.Enums;
using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Concepts.Dtos;

public record ConceptDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    string Name,
    RenderedContent Definition,
    ConceptImpact? ImpactLevel);
