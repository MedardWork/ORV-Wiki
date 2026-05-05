using ORVWiki.Application.Common;
using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class Scenario : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string? Code { get; set; }
    public string Title { get; set; } = null!;
    public ScenarioType Type { get; set; }
    public ScenarioDifficulty Difficulty { get; set; }
    public string? Conditions { get; set; }
    public string? Rewards { get; set; }
    public string? Penalty { get; set; }
    public int ChapterStart { get; set; }
    public int? ChapterEnd { get; set; }

    public ICollection<ScenarioParticipant> ScenarioParticipants { get; set; } = new List<ScenarioParticipant>();
    public ICollection<ScenarioLocation> ScenarioLocations { get; set; } = new List<ScenarioLocation>();
}
