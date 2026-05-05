using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.OuterGods.Dtos;

public record OuterGodDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    string Name,
    string? GodType,
    RenderedContent Description);
