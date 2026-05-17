using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class ConceptContentDescriptor : ContentTypeDescriptor<Concept>
{
    public override EntityType EntityType => EntityType.Concept;
    public override string DisplayName => "Concept";
    protected override DbSet<Concept> Set(IAppDbContext db) => db.Concepts;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Concept>(),
        ContentFields.Text<Concept>("name", "Name", e => e.Name, (e, v) => e.Name = v!,
            required: true, nullable: false, maxLength: 150),
        ContentFields.LongText<Concept>("definition", "Definition", e => e.Definition, (e, v) => e.Definition = v!,
            required: true, nullable: false, maxLength: 8000),
        ContentFields.EnumOfN<Concept, ConceptImpact>("impactLevel", "Impact level",
            e => e.ImpactLevel, (e, v) => e.ImpactLevel = v),
    ];
}
