namespace ORVWiki.Application.OuterGods.Dtos;

public record OuterGodListItemDto(
    long Id,
    string Slug,
    string Name,
    string? GodType,
    int DiscoveryChapter);
