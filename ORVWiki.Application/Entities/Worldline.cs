using ORVWiki.Application.Common;

namespace ORVWiki.Application.Entities;

public class Worldline : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public int LineNumber { get; set; }
    public string? Name { get; set; }
    public long? ParentWorldlineId { get; set; }
    public Worldline? ParentWorldline { get; set; }
    public bool IsMain { get; set; }
    public string? Description { get; set; }

    public ICollection<Worldline> ChildWorldlines { get; set; } = new List<Worldline>();
    public ICollection<Location> Locations { get; set; } = new List<Location>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
