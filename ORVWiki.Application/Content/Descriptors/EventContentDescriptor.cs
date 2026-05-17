using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class EventContentDescriptor : ContentTypeDescriptor<Event>
{
    public override EntityType EntityType => EntityType.Event;
    public override string DisplayName => "Event";
    protected override DbSet<Event> Set(IAppDbContext db) => db.Events;

    protected override IQueryable<Event> ApplyRelationIncludes(IQueryable<Event> query)
        => query.Include(e => e.EventCharacters);

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Event>(),
        ContentFields.Text<Event>("title", "Title", e => e.Title, (e, v) => e.Title = v!,
            required: true, nullable: false, maxLength: 255),
        ContentFields.Int<Event>("chapterNumber", "Chapter number", e => e.ChapterNumber,
            (e, v) => e.ChapterNumber = v),
        ContentFields.EnumOf<Event, EventImportance>("importance", "Importance",
            e => e.Importance, (e, v) => e.Importance = v),
        ContentFields.Ref<Event>("locationId", "Location", EntityType.Location,
            e => e.LocationId, (e, v) => e.LocationId = v),
        ContentFields.Ref<Event>("worldlineId", "Worldline", EntityType.Worldline,
            e => e.WorldlineId, (e, v) => e.WorldlineId = v),
        ContentFields.Text<Event>("lengthEstimate", "Length estimate", e => e.LengthEstimate,
            (e, v) => e.LengthEstimate = v, maxLength: 80),
        ContentFields.LongText<Event>("description", "Description", e => e.Description, (e, v) => e.Description = v,
            maxLength: 8000),
    ];

    public override IReadOnlyList<ContentRelation> Relations { get; } =
    [
        ContentRelations.Of<Event, EventCharacter>("characters", "Characters", EntityType.Character,
            e => e.EventCharacters, p => p.CharacterId, (p, id) => p.CharacterId = id,
            ContentFields.EnumOf<EventCharacter, EventCharacterRole>("role", "Role",
                r => r.Role, (r, v) => r.Role = v)),
    ];
}
