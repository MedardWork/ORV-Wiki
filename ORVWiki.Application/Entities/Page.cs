using System.Text.Json;
using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class Page
{
    public long Id { get; set; }
    public string Slug { get; set; } = null!;
    public string Title { get; set; } = null!;
    public EntityType EntityType { get; set; }
    public int DiscoveryChapter { get; set; } = 1;
    public JsonDocument? SpoilerMap { get; set; }
    public string? ShortDescription { get; set; }
    public int ViewCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<EditSuggestion> EditSuggestions { get; set; } = new List<EditSuggestion>();
    public ICollection<PageTag> PageTags { get; set; } = new List<PageTag>();
}
