using ORVWiki.Application.Enums;
using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Constellations.Dtos;

public record ConstellationDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    string Modifier,
    string? TrueName,
    long? NebulaId,
    ConstellationGrade Grade,
    long? OriginCharacterId,
    RenderedContent Description);
