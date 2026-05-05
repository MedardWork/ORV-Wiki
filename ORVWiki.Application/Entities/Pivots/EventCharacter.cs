using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities.Pivots;

public class EventCharacter
{
    public long Id { get; set; }
    public long EventId { get; set; }
    public Event Event { get; set; } = null!;
    public long CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public EventCharacterRole Role { get; set; }
}
