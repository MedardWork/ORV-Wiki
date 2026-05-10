using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Timeline.Dtos;

namespace ORVWiki.Application.Timeline;

public class TimelineService(IAppDbContext db) : ITimelineService
{
    public async Task<TimelineDto> GetGraphAsync(
        int? upToChapter, long? characterId, CancellationToken ct = default)
    {
        // Every worldline is always returned so the renderer can draw lanes
        // even when no event or jump matches the filter.
        var worldlines = await db.Worldlines
            .AsNoTracking()
            .OrderBy(w => w.DisplayOrder).ThenBy(w => w.LineNumber)
            .Select(w => new WorldlineNodeDto(
                w.Id, w.LineNumber, w.Name, w.IsMain, w.ParentWorldlineId,
                w.Color, w.DisplayOrder))
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
                e.LocationId, e.Importance, e.EventOrder, e.LengthEstimate))
            .ToListAsync(ct);

        // Jumps don't carry a chapter directly; gate them by their Arc when set.
        // Character filter doesn't apply since CharacterLabel is an opaque string.
        var jumpsQuery = db.Jumps.AsNoTracking().AsQueryable();
        if (upToChapter.HasValue)
            jumpsQuery = jumpsQuery.Where(j =>
                j.ArcId == null || j.Arc!.ChapterStart <= upToChapter.Value);

        var jumps = await jumpsQuery
            .OrderBy(j => j.SourceWorldlineId).ThenBy(j => j.SourceOrder)
            .Select(j => new JumpEdgeDto(
                j.Id, j.CharacterLabel, j.Description, j.LengthEstimate,
                j.SourceWorldlineId, j.SourceOrder,
                j.TargetWorldlineId, j.TargetOrder,
                j.ArcId))
            .ToListAsync(ct);

        return new TimelineDto(worldlines, events, jumps);
    }
}
