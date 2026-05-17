using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class CharacterContentDescriptor : ContentTypeDescriptor<Character>
{
    public override EntityType EntityType => EntityType.Character;
    public override string DisplayName => "Character";
    protected override DbSet<Character> Set(IAppDbContext db) => db.Characters;

    protected override IQueryable<Character> ApplyRelationIncludes(IQueryable<Character> query) => query
        .Include(c => c.CharacterStigmas)
        .Include(c => c.CharacterAttributes)
        .Include(c => c.CharacterSkills)
        .Include(c => c.CharacterItems)
        .Include(c => c.CharacterFables)
        .Include(c => c.CharacterConstellations);

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Character>(),
        ContentFields.Text<Character>("fullName", "Full name", e => e.FullName, (e, v) => e.FullName = v!,
            required: true, nullable: false, maxLength: 150),
        ContentFields.Text<Character>("alias", "Alias", e => e.Alias, (e, v) => e.Alias = v, maxLength: 150),
        ContentFields.Text<Character>("species", "Species", e => e.Species, (e, v) => e.Species = v, maxLength: 80),
        ContentFields.EnumOf<Character, CharacterStatus>("status", "Status", e => e.Status, (e, v) => e.Status = v),
        ContentFields.EnumOfN<Character, Gender>("gender", "Gender", e => e.Gender, (e, v) => e.Gender = v),
        ContentFields.IntN<Character>("birthChapter", "Birth chapter", e => e.BirthChapter,
            (e, v) => e.BirthChapter = v),
        ContentFields.IntN<Character>("deathChapter", "Death chapter", e => e.DeathChapter,
            (e, v) => e.DeathChapter = v),
        ContentFields.LongText<Character>("biography", "Biography", e => e.Biography, (e, v) => e.Biography = v,
            maxLength: 20000),
    ];

    public override IReadOnlyList<ContentRelation> Relations { get; } =
    [
        ContentRelations.Of<Character, CharacterStigma>("stigmas", "Stigmas", EntityType.Stigma,
            c => c.CharacterStigmas, p => p.StigmaId, (p, id) => p.StigmaId = id,
            ContentFields.Bool<CharacterStigma>("isPrimary", "Primary", r => r.IsPrimary, (r, v) => r.IsPrimary = v),
            ContentFields.IntN<CharacterStigma>("acquiredChapter", "Acquired chapter",
                r => r.AcquiredChapter, (r, v) => r.AcquiredChapter = v)),
        ContentRelations.Of<Character, CharacterAttribute>("attributes", "Attributes", EntityType.Attribute,
            c => c.CharacterAttributes, p => p.AttributeId, (p, id) => p.AttributeId = id,
            ContentFields.IntN<CharacterAttribute>("acquiredChapter", "Acquired chapter",
                r => r.AcquiredChapter, (r, v) => r.AcquiredChapter = v)),
        ContentRelations.Of<Character, CharacterSkill>("skills", "Skills", EntityType.Skill,
            c => c.CharacterSkills, p => p.SkillId, (p, id) => p.SkillId = id,
            ContentFields.Short<CharacterSkill>("level", "Level", r => r.Level, (r, v) => r.Level = v),
            ContentFields.IntN<CharacterSkill>("acquiredChapter", "Acquired chapter",
                r => r.AcquiredChapter, (r, v) => r.AcquiredChapter = v)),
        ContentRelations.Of<Character, CharacterItem>("items", "Items", EntityType.Item,
            c => c.CharacterItems, p => p.ItemId, (p, id) => p.ItemId = id,
            ContentFields.IntN<CharacterItem>("acquiredChapter", "Acquired chapter",
                r => r.AcquiredChapter, (r, v) => r.AcquiredChapter = v),
            ContentFields.IntN<CharacterItem>("lostChapter", "Lost chapter",
                r => r.LostChapter, (r, v) => r.LostChapter = v)),
        ContentRelations.Of<Character, CharacterFable>("fables", "Fables", EntityType.Fable,
            c => c.CharacterFables, p => p.FableId, (p, id) => p.FableId = id,
            ContentFields.IntN<CharacterFable>("acquiredChapter", "Acquired chapter",
                r => r.AcquiredChapter, (r, v) => r.AcquiredChapter = v)),
        ContentRelations.Of<Character, CharacterConstellation>("constellations", "Constellations",
            EntityType.Constellation,
            c => c.CharacterConstellations, p => p.ConstellationId, (p, id) => p.ConstellationId = id,
            ContentFields.EnumOf<CharacterConstellation, CharacterConstellationRel>("relationshipType",
                "Relationship", r => r.RelationshipType, (r, v) => r.RelationshipType = v),
            ContentFields.IntN<CharacterConstellation>("sinceChapter", "Since chapter",
                r => r.SinceChapter, (r, v) => r.SinceChapter = v)),
    ];

    public override IEnumerable<string> ValidateCrossFields(IPagedEntity entity)
    {
        var c = (Character)entity;
        return c is { BirthChapter: { } b, DeathChapter: { } d } && d < b
            ? ["Death chapter cannot be earlier than birth chapter."]
            : [];
    }
}
