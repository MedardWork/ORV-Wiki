using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Arcs.Dtos;

public record ArcDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    string Name,
    short OrderNumber,
    int ChapterStart,
    int ChapterEnd,
    RenderedContent Summary);
