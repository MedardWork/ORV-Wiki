using System.Text.RegularExpressions;
using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Spoilers;

public partial class SpoilerService : ISpoilerService
{
    [GeneratedRegex(
        pattern: @"\[spoiler\s+ch=(?<ch>\d+)\](?<body>.*?)\[/spoiler\]",
        options: RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SpoilerRegex();

    public bool IsRevealed(int discoveryChapter, int currentChapter)
        => discoveryChapter <= currentChapter;

    public void EnsureRevealed(int discoveryChapter, int currentChapter, string entityName = "Entity")
    {
        if (!IsRevealed(discoveryChapter, currentChapter))
            throw new NotFoundException($"{entityName} not found.");
    }

    public RenderedContent RenderInline(string? text, int currentChapter)
    {
        if (string.IsNullOrEmpty(text))
            return RenderedContent.Empty;

        var segments = new List<Segment>();
        var cursor = 0;

        foreach (Match match in SpoilerRegex().Matches(text))
        {
            if (match.Index > cursor)
                segments.Add(Segment.AsText(text[cursor..match.Index]));

            var revealChapter = int.Parse(match.Groups["ch"].Value);
            var body = match.Groups["body"].Value;

            segments.Add(currentChapter >= revealChapter
                ? Segment.AsRevealedSpoiler(body, revealChapter)
                : Segment.AsHiddenSpoiler(revealChapter));

            cursor = match.Index + match.Length;
        }

        if (cursor < text.Length)
            segments.Add(Segment.AsText(text[cursor..]));

        return new RenderedContent(segments);
    }
}
