namespace ORVWiki.Application.Entities.Pivots;

public class CharacterSkill
{
    public long Id { get; set; }
    public long CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public long SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
    public short Level { get; set; } = 1;
    public int? AcquiredChapter { get; set; }
}
