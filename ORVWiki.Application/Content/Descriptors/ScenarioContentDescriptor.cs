using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class ScenarioContentDescriptor : ContentTypeDescriptor<Scenario>
{
    public override EntityType EntityType => EntityType.Scenario;
    public override string DisplayName => "Scenario";
    protected override DbSet<Scenario> Set(IAppDbContext db) => db.Scenarios;

    protected override IQueryable<Scenario> ApplyRelationIncludes(IQueryable<Scenario> query)
        => query.Include(s => s.ScenarioParticipants).Include(s => s.ScenarioLocations);

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Scenario>(),
        ContentFields.Text<Scenario>("title", "Title", e => e.Title, (e, v) => e.Title = v!,
            required: true, nullable: false, maxLength: 255),
        ContentFields.Text<Scenario>("code", "Code", e => e.Code, (e, v) => e.Code = v, maxLength: 40),
        ContentFields.EnumOf<Scenario, ScenarioType>("type", "Type", e => e.Type, (e, v) => e.Type = v),
        ContentFields.EnumOf<Scenario, ScenarioDifficulty>("difficulty", "Difficulty",
            e => e.Difficulty, (e, v) => e.Difficulty = v),
        ContentFields.Int<Scenario>("chapterStart", "Chapter start", e => e.ChapterStart,
            (e, v) => e.ChapterStart = v),
        ContentFields.IntN<Scenario>("chapterEnd", "Chapter end", e => e.ChapterEnd, (e, v) => e.ChapterEnd = v),
        ContentFields.LongText<Scenario>("conditions", "Conditions", e => e.Conditions, (e, v) => e.Conditions = v,
            maxLength: 8000),
        ContentFields.LongText<Scenario>("rewards", "Rewards", e => e.Rewards, (e, v) => e.Rewards = v,
            maxLength: 8000),
        ContentFields.LongText<Scenario>("penalty", "Penalty", e => e.Penalty, (e, v) => e.Penalty = v,
            maxLength: 8000),
    ];

    public override IReadOnlyList<ContentRelation> Relations { get; } =
    [
        ContentRelations.Of<Scenario, ScenarioParticipant>("participants", "Participants", EntityType.Character,
            s => s.ScenarioParticipants, p => p.CharacterId, (p, id) => p.CharacterId = id,
            ContentFields.EnumOf<ScenarioParticipant, ScenarioOutcome>("outcome", "Outcome",
                r => r.Outcome, (r, v) => r.Outcome = v)),
        ContentRelations.Of<Scenario, ScenarioLocation>("locations", "Locations", EntityType.Location,
            s => s.ScenarioLocations, p => p.LocationId, (p, id) => p.LocationId = id),
    ];
}
