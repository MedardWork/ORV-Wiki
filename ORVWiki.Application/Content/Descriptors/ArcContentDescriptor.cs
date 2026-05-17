using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class ArcContentDescriptor : ContentTypeDescriptor<Arc>
{
    public override EntityType EntityType => EntityType.Arc;
    public override string DisplayName => "Arc";
    protected override DbSet<Arc> Set(IAppDbContext db) => db.Arcs;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Arc>(),
        ContentFields.Text<Arc>("name", "Name", e => e.Name, (e, v) => e.Name = v!,
            required: true, nullable: false, maxLength: 200),
        ContentFields.Short<Arc>("orderNumber", "Order number", e => e.OrderNumber, (e, v) => e.OrderNumber = v),
        ContentFields.Int<Arc>("chapterStart", "Chapter start", e => e.ChapterStart, (e, v) => e.ChapterStart = v),
        ContentFields.Int<Arc>("chapterEnd", "Chapter end", e => e.ChapterEnd, (e, v) => e.ChapterEnd = v),
        ContentFields.LongText<Arc>("summary", "Summary", e => e.Summary, (e, v) => e.Summary = v, maxLength: 8000),
    ];
}
