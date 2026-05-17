using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class NebulaContentDescriptor : ContentTypeDescriptor<Nebula>
{
    public override EntityType EntityType => EntityType.Nebula;
    public override string DisplayName => "Nebula";
    protected override DbSet<Nebula> Set(IAppDbContext db) => db.Nebulae;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Nebula>(),
        ContentFields.Text<Nebula>("name", "Name", e => e.Name, (e, v) => e.Name = v!,
            required: true, nullable: false, maxLength: 200),
        ContentFields.Ref<Nebula>("founderConstellationId", "Founder constellation", EntityType.Constellation,
            e => e.FounderConstellationId, (e, v) => e.FounderConstellationId = v),
        ContentFields.ShortN<Nebula>("powerRank", "Power rank", e => e.PowerRank, (e, v) => e.PowerRank = v),
        ContentFields.LongText<Nebula>("description", "Description", e => e.Description, (e, v) => e.Description = v,
            maxLength: 8000),
    ];
}
