using ORVWiki.Application.Common;
using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class Skill : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string Name { get; set; } = null!;
    public SkillType SkillType { get; set; }
    public int? CostInCoins { get; set; }
    public short? MaxLevel { get; set; } = 10;
    public string? Effect { get; set; }

    public ICollection<CharacterSkill> CharacterSkills { get; set; } = new List<CharacterSkill>();
}
