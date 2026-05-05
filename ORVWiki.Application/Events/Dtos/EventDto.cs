using ORVWiki.Application.Enums;
using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Events.Dtos;

public record EventDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    RenderedContent EventTitle,
    RenderedContent Description,
    int ChapterNumber,
    long? LocationId,
    long? WorldlineId,
    EventImportance Importance,
    int? EventOrder);
