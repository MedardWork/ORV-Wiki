using ORVWiki.Application.Common;
using ORVWiki.Application.Entities.Pivots;

namespace ORVWiki.Application.Entities;

public class Stigma : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string Name { get; set; } = null!;
    public long ProviderConstellationId { get; set; }
    public Constellation ProviderConstellation { get; set; } = null!;
    public int ActivationCost { get; set; }
    public string Effect { get; set; } = null!;

    public ICollection<CharacterStigma> CharacterStigmas { get; set; } = new List<CharacterStigma>();
}
