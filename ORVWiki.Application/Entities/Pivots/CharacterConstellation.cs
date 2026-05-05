using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities.Pivots;

public class CharacterConstellation
{
    public long Id { get; set; }
    public long CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public long ConstellationId { get; set; }
    public Constellation Constellation { get; set; } = null!;
    public CharacterConstellationRel RelationshipType { get; set; }
    public int? SinceChapter { get; set; }
}
