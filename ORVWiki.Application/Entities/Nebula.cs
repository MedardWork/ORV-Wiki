using ORVWiki.Application.Common;

namespace ORVWiki.Application.Entities;

public class Nebula : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string Name { get; set; } = null!;
    public long? FounderConstellationId { get; set; }
    public Constellation? FounderConstellation { get; set; }
    public string? Description { get; set; }
    public short? PowerRank { get; set; }

    public ICollection<Constellation> Constellations { get; set; } = new List<Constellation>();
}
