namespace ORVWiki.Application.Nebulae.Dtos;

public record NebulaListItemDto(
    long Id,
    string Slug,
    string Name,
    short? PowerRank,
    int DiscoveryChapter);
