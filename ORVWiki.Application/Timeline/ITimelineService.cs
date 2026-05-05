using ORVWiki.Application.Timeline.Dtos;

namespace ORVWiki.Application.Timeline;

public interface ITimelineService
{
    /// <summary>
    /// Returns the full graph payload for the 3D timeline renderer:
    /// all worldlines (with parent chain), all events (optionally chapter- and
    /// character-filtered), and all event connections that touch the resulting
    /// event set. Per spec §9.2 the timeline is inherently spoiler-rich; chapter
    /// filtering is opt-in.
    /// </summary>
    Task<TimelineDto> GetGraphAsync(int? upToChapter, long? characterId, CancellationToken ct = default);
}
