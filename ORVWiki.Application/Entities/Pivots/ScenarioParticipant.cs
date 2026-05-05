using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities.Pivots;

public class ScenarioParticipant
{
    public long Id { get; set; }
    public long ScenarioId { get; set; }
    public Scenario Scenario { get; set; } = null!;
    public long CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public ScenarioOutcome Outcome { get; set; }
}
