using ORVWiki.Application.Enums;
using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Fables.Dtos;

public record FableDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    RenderedContent FableTitle,
    FableGrade Grade,
    RenderedContent Legend,
    long? OriginCharacterId);
