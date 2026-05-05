namespace ORVWiki.Application.Spoilers.Dtos;

public record RenderedContent(IReadOnlyList<Segment> Segments)
{
    public bool HasHiddenContent =>
        Segments.Any(s => s.Type == Segment.Spoiler && s.Content is null);

    public static RenderedContent Empty { get; } = new(Array.Empty<Segment>());
}
