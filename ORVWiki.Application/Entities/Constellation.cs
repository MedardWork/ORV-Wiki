using ORVWiki.Application.Common;
using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class Constellation : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string Modifier { get; set; } = null!;
    public string? TrueName { get; set; }
    public long? NebulaId { get; set; }
    public Nebula? Nebula { get; set; }
    public ConstellationGrade Grade { get; set; }
    public long? OriginCharacterId { get; set; }
    public Character? OriginCharacter { get; set; }
    public string? Description { get; set; }

    public ICollection<Stigma> ProvidedStigmas { get; set; } = new List<Stigma>();
    public ICollection<CharacterConstellation> CharacterConstellations { get; set; } = new List<CharacterConstellation>();
}
