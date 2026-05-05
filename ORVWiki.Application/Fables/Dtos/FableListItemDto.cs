using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Fables.Dtos;

public record FableListItemDto(
    long Id,
    string Slug,
    string Title,
    FableGrade Grade,
    int DiscoveryChapter);
