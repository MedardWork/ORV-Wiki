using ORVWiki.Application.Common;
using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class Attribute : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string Name { get; set; } = null!;
    public AttributeRarity Rarity { get; set; } = AttributeRarity.Common;
    public string? Effect { get; set; }

    public ICollection<CharacterAttribute> CharacterAttributes { get; set; } = new List<CharacterAttribute>();
}
