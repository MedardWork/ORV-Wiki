using ORVWiki.Application.Common;
using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class Event : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int ChapterNumber { get; set; }
    public Chapter Chapter { get; set; } = null!;
    public long? LocationId { get; set; }
    public Location? Location { get; set; }
    public long? WorldlineId { get; set; }
    public Worldline? Worldline { get; set; }
    public EventImportance Importance { get; set; } = EventImportance.Minor;
    public int? EventOrder { get; set; }

    public ICollection<EventCharacter> EventCharacters { get; set; } = new List<EventCharacter>();
    public ICollection<EventConnection> OutgoingConnections { get; set; } = new List<EventConnection>();
    public ICollection<EventConnection> IncomingConnections { get; set; } = new List<EventConnection>();
}
