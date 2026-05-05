namespace ORVWiki.Application.Entities.Pivots;

public class PageTag
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;
    public short TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
