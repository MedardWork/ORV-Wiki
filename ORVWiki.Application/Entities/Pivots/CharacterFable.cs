namespace ORVWiki.Application.Entities.Pivots;

public class CharacterFable
{
    public long Id { get; set; }
    public long CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public long FableId { get; set; }
    public Fable Fable { get; set; } = null!;
    public int? AcquiredChapter { get; set; }
}
