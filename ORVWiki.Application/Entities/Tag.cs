using ORVWiki.Application.Entities.Pivots;

namespace ORVWiki.Application.Entities;

public class Tag
{
    public short Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Color { get; set; }

    public ICollection<PageTag> PageTags { get; set; } = new List<PageTag>();
}
