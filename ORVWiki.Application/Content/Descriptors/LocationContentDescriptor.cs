using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class LocationContentDescriptor : ContentTypeDescriptor<Location>
{
    public override EntityType EntityType => EntityType.Location;
    public override string DisplayName => "Location";
    protected override DbSet<Location> Set(IAppDbContext db) => db.Locations;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Location>(),
        ContentFields.Text<Location>("name", "Name", e => e.Name, (e, v) => e.Name = v!,
            required: true, nullable: false, maxLength: 200),
        ContentFields.Text<Location>("dimension", "Dimension", e => e.Dimension, (e, v) => e.Dimension = v,
            maxLength: 120),
        ContentFields.Ref<Location>("worldlineId", "Worldline", EntityType.Worldline,
            e => e.WorldlineId, (e, v) => e.WorldlineId = v),
        ContentFields.Ref<Location>("parentLocationId", "Parent location", EntityType.Location,
            e => e.ParentLocationId, (e, v) => e.ParentLocationId = v),
        ContentFields.LongText<Location>("description", "Description", e => e.Description, (e, v) => e.Description = v,
            maxLength: 8000),
    ];
}
