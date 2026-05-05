using ORVWiki.Application.Common;

namespace ORVWiki.Application.Entities;

public class OuterGod : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? GodType { get; set; }
    public string? Description { get; set; }
}
