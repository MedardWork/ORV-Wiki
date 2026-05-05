using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.DemonKings.Dtos;

public record DemonKingDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    short Ranking,
    string Name,
    string? DemonRealm,
    RenderedContent Description);
