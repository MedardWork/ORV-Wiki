namespace ORVWiki.Application.DemonKings.Dtos;

public record DemonKingListItemDto(
    long Id,
    string Slug,
    short Ranking,
    string Name,
    int DiscoveryChapter);
