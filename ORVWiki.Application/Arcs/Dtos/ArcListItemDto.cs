namespace ORVWiki.Application.Arcs.Dtos;

public record ArcListItemDto(
    long Id,
    string Slug,
    string Name,
    short OrderNumber,
    int ChapterStart,
    int ChapterEnd,
    int DiscoveryChapter);
