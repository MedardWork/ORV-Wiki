namespace ORVWiki.Application.Entities.Pivots;

public class CharacterItem
{
    public long Id { get; set; }
    public long CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public long ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public int? AcquiredChapter { get; set; }
    public int? LostChapter { get; set; }
}
