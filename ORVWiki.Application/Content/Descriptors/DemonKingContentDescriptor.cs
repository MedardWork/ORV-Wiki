using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class DemonKingContentDescriptor : ContentTypeDescriptor<DemonKing>
{
    public override EntityType EntityType => EntityType.DemonKing;
    public override string DisplayName => "Demon King";
    protected override DbSet<DemonKing> Set(IAppDbContext db) => db.DemonKings;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<DemonKing>(),
        ContentFields.Text<DemonKing>("name", "Name", e => e.Name, (e, v) => e.Name = v!,
            required: true, nullable: false, maxLength: 150),
        ContentFields.Short<DemonKing>("ranking", "Ranking", e => e.Ranking, (e, v) => e.Ranking = v),
        ContentFields.Text<DemonKing>("demonRealm", "Demon realm", e => e.DemonRealm, (e, v) => e.DemonRealm = v,
            maxLength: 200),
        ContentFields.LongText<DemonKing>("description", "Description", e => e.Description, (e, v) => e.Description = v,
            maxLength: 8000),
    ];
}
