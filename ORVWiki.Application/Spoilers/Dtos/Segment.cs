namespace ORVWiki.Application.Spoilers.Dtos;

/// <summary>
/// One piece of rendered content. Type is "text" or "spoiler".
/// For "spoiler" segments below the caller's current_chapter, Content is null
/// (server-enforced — hidden content never reaches the client).
/// </summary>
public record Segment(string Type, string? Content, int? RevealChapter)
{
    public const string Text = "text";
    public const string Spoiler = "spoiler";

    public static Segment AsText(string content) => new(Text, content, null);
    public static Segment AsRevealedSpoiler(string content, int revealChapter)
        => new(Spoiler, content, revealChapter);
    public static Segment AsHiddenSpoiler(int revealChapter)
        => new(Spoiler, null, revealChapter);
}
