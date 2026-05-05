using ORVWiki.Application.Common;
using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class Fable : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string Title { get; set; } = null!;
    public FableGrade Grade { get; set; }
    public string? Legend { get; set; }
    public long? OriginCharacterId { get; set; }
    public Character? OriginCharacter { get; set; }

    public ICollection<CharacterFable> CharacterFables { get; set; } = new List<CharacterFable>();
}
