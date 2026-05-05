using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Constellations.Dtos;

public record ConstellationListItemDto(
    long Id,
    string Slug,
    string Modifier,
    string? TrueName,
    ConstellationGrade Grade,
    int DiscoveryChapter);
