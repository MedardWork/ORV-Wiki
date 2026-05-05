namespace ORVWiki.Application.Entities.Pivots;

public class CharacterAttribute
{
    public long Id { get; set; }
    public long CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public long AttributeId { get; set; }
    public Attribute Attribute { get; set; } = null!;
    public int? AcquiredChapter { get; set; }
}
