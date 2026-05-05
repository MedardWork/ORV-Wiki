namespace ORVWiki.Application.Worldlines.Dtos;

public record WorldlineListItemDto(
    long Id,
    string Slug,
    int LineNumber,
    string? Name,
    bool IsMain,
    int DiscoveryChapter);
