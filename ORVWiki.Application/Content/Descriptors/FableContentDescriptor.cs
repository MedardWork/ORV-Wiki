using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class FableContentDescriptor : ContentTypeDescriptor<Fable>
{
    public override EntityType EntityType => EntityType.Fable;
    public override string DisplayName => "Fable";
    protected override DbSet<Fable> Set(IAppDbContext db) => db.Fables;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Fable>(),
        ContentFields.Text<Fable>("title", "Title", e => e.Title, (e, v) => e.Title = v!,
            required: true, nullable: false, maxLength: 200),
        ContentFields.EnumOf<Fable, FableGrade>("grade", "Grade", e => e.Grade, (e, v) => e.Grade = v),
        ContentFields.LongText<Fable>("legend", "Legend", e => e.Legend, (e, v) => e.Legend = v, maxLength: 8000),
        ContentFields.Ref<Fable>("originCharacterId", "Origin character", EntityType.Character,
            e => e.OriginCharacterId, (e, v) => e.OriginCharacterId = v),
    ];
}
