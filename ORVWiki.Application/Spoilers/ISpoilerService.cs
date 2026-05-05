using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Spoilers;

public interface ISpoilerService
{
    /// <summary>
    /// Page-level gate: returns true iff the entity's <c>discovery_chapter</c>
    /// is at or below the caller's current chapter.
    /// </summary>
    bool IsRevealed(int discoveryChapter, int currentChapter);

    /// <summary>
    /// Page-level gate variant that throws <see cref="Common.Exceptions.NotFoundException"/>
    /// when the entity is not yet revealed (so the caller sees a 404).
    /// </summary>
    void EnsureRevealed(int discoveryChapter, int currentChapter, string entityName = "Entity");

    /// <summary>
    /// Parses raw text containing <c>[spoiler ch=N]...[/spoiler]</c> markup
    /// into structured segments. Hidden spoilers carry no content.
    /// </summary>
    RenderedContent RenderInline(string? text, int currentChapter);
}
