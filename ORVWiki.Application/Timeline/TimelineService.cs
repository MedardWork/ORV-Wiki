using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Timeline.Dtos;

namespace ORVWiki.Application.Timeline;

public class TimelineService(IAppDbContext db) : ITimelineService
{
    public async Task<TimelineDto> GetGraphAsync(
        int? upToChapter, long? characterId, CancellationToken ct = default)
    {
        // Skeleton: every worldline is always returned so the renderer can draw
        // the parent chain even when no event matches the filter.
        var worldlines = await db.Worldlines
            .AsNoTracking()
            .OrderBy(w => w.LineNumber)
            .Select(w => new WorldlineNodeDto(
                w.Id, w.LineNumber, w.Name, w.IsMain, w.ParentWorldlineId))
            .ToListAsync(ct);

        var eventsQuery = db.Events.AsNoTracking().AsQueryable();
        if (upToChapter.HasValue)
            eventsQuery = eventsQuery.Where(e => e.ChapterNumber <= upToChapter.Value);
        if (characterId.HasValue)
            eventsQuery = eventsQuery.Where(e =>
                e.EventCharacters.Any(ec => ec.CharacterId == characterId.Value));

        var events = await eventsQuery
            .OrderBy(e => e.ChapterNumber).ThenBy(e => e.EventOrder)
            .Select(e => new EventNodeDto(
                e.Id, e.Title, e.ChapterNumber, e.WorldlineId,
                e.LocationId, e.Importance, e.EventOrder))
            .ToListAsync(ct);

        // Edges only between events present in the result set — orphaned edges
        // would be unrenderable.
        var eventIds = events.Select(e => e.Id).ToHashSet();
        var connections = eventIds.Count == 0
            ? []
            : await db.EventConnections
                .AsNoTracking()
                .Where(c => eventIds.Contains(c.SourceEventId)
                            && eventIds.Contains(c.TargetEventId))
                .Select(c => new EventConnectionEdgeDto(
                    c.Id, c.SourceEventId, c.TargetEventId,
                    c.ConnectionType, c.CharacterId, c.Description))
                .ToListAsync(ct);

        return new TimelineDto(worldlines, events, connections);
    }
}
