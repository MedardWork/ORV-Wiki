namespace ORVWiki.Application.Entities.Pivots;

public class ScenarioLocation
{
    public long Id { get; set; }
    public long ScenarioId { get; set; }
    public Scenario Scenario { get; set; } = null!;
    public long LocationId { get; set; }
    public Location Location { get; set; } = null!;
}
