namespace ORVWiki.Application.Entities.Pivots;

public class CharacterStigma
{
    public long Id { get; set; }
    public long CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public long StigmaId { get; set; }
    public Stigma Stigma { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public int? AcquiredChapter { get; set; }
}
