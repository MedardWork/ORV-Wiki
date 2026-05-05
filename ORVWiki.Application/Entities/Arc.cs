using ORVWiki.Application.Common;

namespace ORVWiki.Application.Entities;

public class Arc : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string Name { get; set; } = null!;
    public short OrderNumber { get; set; }
    public int ChapterStart { get; set; }
    public int ChapterEnd { get; set; }
    public string? Summary { get; set; }

    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
}
