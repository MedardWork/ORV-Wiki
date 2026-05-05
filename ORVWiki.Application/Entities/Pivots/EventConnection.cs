using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities.Pivots;

public class EventConnection
{
    public long Id { get; set; }
    public long SourceEventId { get; set; }
    public Event SourceEvent { get; set; } = null!;
    public long TargetEventId { get; set; }
    public Event TargetEvent { get; set; } = null!;
    public EventConnectionType ConnectionType { get; set; }
    public long? CharacterId { get; set; }
    public Character? Character { get; set; }
    public string? Description { get; set; }
}
