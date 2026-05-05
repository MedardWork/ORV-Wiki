using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Events.Dtos;

public record EventListItemDto(
    long Id,
    string Slug,
    string Title,
    int ChapterNumber,
    EventImportance Importance,
    int DiscoveryChapter);
