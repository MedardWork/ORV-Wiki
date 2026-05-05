using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Timeline.Dtos;

public record EventNodeDto(
    long Id,
    string Title,
    int ChapterNumber,
    long? WorldlineId,
    long? LocationId,
    EventImportance Importance,
    int? EventOrder);
