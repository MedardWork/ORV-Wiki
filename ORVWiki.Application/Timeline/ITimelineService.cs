using ORVWiki.Application.Timeline.Dtos;

namespace ORVWiki.Application.Timeline;

public interface ITimelineService
{
    /// <summary>
    /// Returns the full graph payload for the timeline renderer:
    /// all worldlines (with parent chain, color, and display order), events
    /// (optionally chapter- and character-filtered), and worldline jumps
    /// (optionally chapter-filtered via their Arc). Per spec §9.2 the timeline
    /// is inherently spoiler-rich; chapter filtering is opt-in.
    /// </summary>
    Task<TimelineDto> GetGraphAsync(int? upToChapter, long? characterId, CancellationToken ct = default);
}
