using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Events.Dtos;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.Events;

public class EventService(
    IPagedEntityRepository<Event> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<Event, EventDto, EventListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Event";

    protected override EventDto ToDto(Event e, int currentChapter) => new(
        e.Id,
        e.PageId,
        e.Page.Slug,
        Spoilers.RenderInline(e.Page.Title, currentChapter),
        e.Page.DiscoveryChapter,
        Spoilers.RenderInline(e.Page.ShortDescription, currentChapter),
        Spoilers.RenderInline(e.Title, currentChapter),
        Spoilers.RenderInline(e.Description, currentChapter),
        e.ChapterNumber,
        e.LocationId,
        e.WorldlineId,
        e.Importance,
        e.EventOrder);

    protected override EventListItemDto ToListItem(Event e) => new(
        e.Id,
        e.Page.Slug,
        e.Title,
        e.ChapterNumber,
        e.Importance,
        e.Page.DiscoveryChapter);
}
