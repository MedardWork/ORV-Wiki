using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Nebulae.Dtos;

public record NebulaDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    string Name,
    long? FounderConstellationId,
    RenderedContent Description,
    short? PowerRank);
