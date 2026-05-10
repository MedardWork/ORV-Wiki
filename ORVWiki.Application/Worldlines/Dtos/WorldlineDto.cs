using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Worldlines.Dtos;

public record WorldlineDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    int LineNumber,
    string? Name,
    long? ParentWorldlineId,
    bool IsMain,
    string? Color,
    int DisplayOrder,
    RenderedContent Description);
