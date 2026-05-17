using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class WorldlineContentDescriptor : ContentTypeDescriptor<Worldline>
{
    public override EntityType EntityType => EntityType.Worldline;
    public override string DisplayName => "Worldline";
    protected override DbSet<Worldline> Set(IAppDbContext db) => db.Worldlines;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Worldline>(),
        ContentFields.Int<Worldline>("lineNumber", "Line number", e => e.LineNumber, (e, v) => e.LineNumber = v),
        ContentFields.Text<Worldline>("name", "Name", e => e.Name, (e, v) => e.Name = v, maxLength: 200),
        ContentFields.Ref<Worldline>("parentWorldlineId", "Parent worldline", EntityType.Worldline,
            e => e.ParentWorldlineId, (e, v) => e.ParentWorldlineId = v),
        ContentFields.Bool<Worldline>("isMain", "Main worldline", e => e.IsMain, (e, v) => e.IsMain = v),
        ContentFields.Text<Worldline>("color", "Colour", e => e.Color, (e, v) => e.Color = v, maxLength: 20),
        ContentFields.Int<Worldline>("displayOrder", "Display order", e => e.DisplayOrder,
            (e, v) => e.DisplayOrder = v, required: false),
        ContentFields.LongText<Worldline>("description", "Description", e => e.Description, (e, v) => e.Description = v,
            maxLength: 8000),
    ];
}
