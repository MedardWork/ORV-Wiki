using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Stigmas.Dtos;

public record StigmaDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    string Name,
    long ProviderConstellationId,
    int ActivationCost,
    RenderedContent Effect);
