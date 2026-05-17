using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class OuterGodContentDescriptor : ContentTypeDescriptor<OuterGod>
{
    public override EntityType EntityType => EntityType.OuterGod;
    public override string DisplayName => "Outer God";
    protected override DbSet<OuterGod> Set(IAppDbContext db) => db.OuterGods;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<OuterGod>(),
        ContentFields.Text<OuterGod>("name", "Name", e => e.Name, (e, v) => e.Name = v!,
            required: true, nullable: false, maxLength: 150),
        ContentFields.Text<OuterGod>("godType", "God type", e => e.GodType, (e, v) => e.GodType = v,
            maxLength: 120),
        ContentFields.LongText<OuterGod>("description", "Description", e => e.Description, (e, v) => e.Description = v,
            maxLength: 8000),
    ];
}
