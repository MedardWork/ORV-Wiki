using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Concepts.Dtos;

public record ConceptListItemDto(
    long Id,
    string Slug,
    string Name,
    ConceptImpact? ImpactLevel,
    int DiscoveryChapter);
