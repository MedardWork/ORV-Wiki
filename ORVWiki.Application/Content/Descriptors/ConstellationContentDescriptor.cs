using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class ConstellationContentDescriptor : ContentTypeDescriptor<Constellation>
{
    public override EntityType EntityType => EntityType.Constellation;
    public override string DisplayName => "Constellation";
    protected override DbSet<Constellation> Set(IAppDbContext db) => db.Constellations;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Constellation>(),
        ContentFields.Text<Constellation>("modifier", "Modifier", e => e.Modifier, (e, v) => e.Modifier = v!,
            required: true, nullable: false, maxLength: 200),
        ContentFields.Text<Constellation>("trueName", "True name", e => e.TrueName, (e, v) => e.TrueName = v,
            maxLength: 200),
        ContentFields.EnumOf<Constellation, ConstellationGrade>("grade", "Grade", e => e.Grade, (e, v) => e.Grade = v),
        ContentFields.Ref<Constellation>("nebulaId", "Nebula", EntityType.Nebula,
            e => e.NebulaId, (e, v) => e.NebulaId = v),
        ContentFields.Ref<Constellation>("originCharacterId", "Origin character", EntityType.Character,
            e => e.OriginCharacterId, (e, v) => e.OriginCharacterId = v),
        ContentFields.LongText<Constellation>("description", "Description", e => e.Description,
            (e, v) => e.Description = v, maxLength: 8000),
    ];
}
