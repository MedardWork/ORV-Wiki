using System.Text.Json;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities.Pivots;

namespace ORVWiki.Application.Entities;

public class Location : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? Dimension { get; set; }
    public long? WorldlineId { get; set; }
    public Worldline? Worldline { get; set; }
    public long? ParentLocationId { get; set; }
    public Location? ParentLocation { get; set; }
    public JsonDocument? Coordinates { get; set; }
    public string? Description { get; set; }

    public ICollection<Location> ChildLocations { get; set; } = new List<Location>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<ScenarioLocation> ScenarioLocations { get; set; } = new List<ScenarioLocation>();
}
