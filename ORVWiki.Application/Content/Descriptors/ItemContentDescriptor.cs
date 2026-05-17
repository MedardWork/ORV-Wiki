using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class ItemContentDescriptor : ContentTypeDescriptor<Item>
{
    public override EntityType EntityType => EntityType.Item;
    public override string DisplayName => "Item";
    protected override DbSet<Item> Set(IAppDbContext db) => db.Items;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Item>(),
        ContentFields.Text<Item>("name", "Name", e => e.Name, (e, v) => e.Name = v!,
            required: true, nullable: false, maxLength: 150),
        ContentFields.EnumOf<Item, ItemGrade>("itemGrade", "Grade", e => e.ItemGrade, (e, v) => e.ItemGrade = v),
        ContentFields.Bool<Item>("isStarRelic", "Star relic", e => e.IsStarRelic, (e, v) => e.IsStarRelic = v),
        ContentFields.LongText<Item>("description", "Description", e => e.Description, (e, v) => e.Description = v,
            maxLength: 8000),
    ];
}
