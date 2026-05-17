using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Enums;
using AttributeEntity = ORVWiki.Application.Entities.Attribute;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class AttributeContentDescriptor : ContentTypeDescriptor<AttributeEntity>
{
    public override EntityType EntityType => EntityType.Attribute;
    public override string DisplayName => "Attribute";
    protected override DbSet<AttributeEntity> Set(IAppDbContext db) => db.Attributes;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<AttributeEntity>(),
        ContentFields.Text<AttributeEntity>("name", "Name", e => e.Name, (e, v) => e.Name = v!,
            required: true, nullable: false, maxLength: 150),
        ContentFields.EnumOf<AttributeEntity, AttributeRarity>("rarity", "Rarity",
            e => e.Rarity, (e, v) => e.Rarity = v),
        ContentFields.LongText<AttributeEntity>("effect", "Effect", e => e.Effect, (e, v) => e.Effect = v,
            maxLength: 4000),
    ];
}
