using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class Character
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string FullName { get; set; } = null!;
    public string? Alias { get; set; }
    public string? Species { get; set; } = "human";
    public CharacterStatus Status { get; set; } = CharacterStatus.Unknown;
    public Gender? Gender { get; set; }
    public int? BirthChapter { get; set; }
    public int? DeathChapter { get; set; }
    public string? Biography { get; set; }
    public long? PortraitMediaId { get; set; }
    public Media? PortraitMedia { get; set; }

    public ICollection<CharacterStigma> CharacterStigmas { get; set; } = new List<CharacterStigma>();
    public ICollection<CharacterAttribute> CharacterAttributes { get; set; } = new List<CharacterAttribute>();
    public ICollection<CharacterSkill> CharacterSkills { get; set; } = new List<CharacterSkill>();
    public ICollection<CharacterItem> CharacterItems { get; set; } = new List<CharacterItem>();
    public ICollection<CharacterFable> CharacterFables { get; set; } = new List<CharacterFable>();
    public ICollection<CharacterConstellation> CharacterConstellations { get; set; } = new List<CharacterConstellation>();
    public ICollection<EventCharacter> EventCharacters { get; set; } = new List<EventCharacter>();
    public ICollection<ScenarioParticipant> ScenarioParticipants { get; set; } = new List<ScenarioParticipant>();

    public ICollection<Constellation> DeifiedConstellations { get; set; } = new List<Constellation>();
    public ICollection<Fable> OriginatedFables { get; set; } = new List<Fable>();
    public ICollection<EventConnection> EventConnections { get; set; } = new List<EventConnection>();
}
