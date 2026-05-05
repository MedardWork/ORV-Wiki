using ORVWiki.Application.Common;

namespace ORVWiki.Application.Entities;

public class DemonKing : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public short Ranking { get; set; }
    public string Name { get; set; } = null!;
    public string? DemonRealm { get; set; }
    public string? Description { get; set; }
}
