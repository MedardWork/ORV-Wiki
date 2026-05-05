namespace ORVWiki.Application.Locations.Dtos;

public record LocationListItemDto(
    long Id,
    string Slug,
    string Name,
    string? Dimension,
    int DiscoveryChapter);
