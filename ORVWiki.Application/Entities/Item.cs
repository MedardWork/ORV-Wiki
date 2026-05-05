using ORVWiki.Application.Common;
using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class Item : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string Name { get; set; } = null!;
    public ItemGrade ItemGrade { get; set; }
    public bool IsStarRelic { get; set; }
    public string? Description { get; set; }

    public ICollection<CharacterItem> CharacterItems { get; set; } = new List<CharacterItem>();
}
