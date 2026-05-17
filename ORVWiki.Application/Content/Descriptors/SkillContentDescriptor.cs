using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class SkillContentDescriptor : ContentTypeDescriptor<Skill>
{
    public override EntityType EntityType => EntityType.Skill;
    public override string DisplayName => "Skill";
    protected override DbSet<Skill> Set(IAppDbContext db) => db.Skills;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Skill>(),
        ContentFields.Text<Skill>("name", "Name", e => e.Name, (e, v) => e.Name = v!,
            required: true, nullable: false, maxLength: 150),
        ContentFields.EnumOf<Skill, SkillType>("skillType", "Skill type", e => e.SkillType, (e, v) => e.SkillType = v),
        ContentFields.IntN<Skill>("costInCoins", "Cost in coins", e => e.CostInCoins, (e, v) => e.CostInCoins = v),
        ContentFields.ShortN<Skill>("maxLevel", "Max level", e => e.MaxLevel, (e, v) => e.MaxLevel = v),
        ContentFields.LongText<Skill>("effect", "Effect", e => e.Effect, (e, v) => e.Effect = v, maxLength: 4000),
    ];
}
