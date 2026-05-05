using ORVWiki.Application.Common;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class Concept : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string Definition { get; set; } = null!;
    public ConceptImpact? ImpactLevel { get; set; } = ConceptImpact.Medium;
}
