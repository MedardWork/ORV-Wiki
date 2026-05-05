namespace ORVWiki.Application.Stigmas.Dtos;

public record StigmaListItemDto(
    long Id,
    string Slug,
    string Name,
    long ProviderConstellationId,
    int DiscoveryChapter);
