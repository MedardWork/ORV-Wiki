using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class StigmaContentDescriptor : ContentTypeDescriptor<Stigma>
{
    public override EntityType EntityType => EntityType.Stigma;
    public override string DisplayName => "Stigma";
    protected override DbSet<Stigma> Set(IAppDbContext db) => db.Stigmas;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Stigma>(),
        ContentFields.Text<Stigma>("name", "Name", e => e.Name, (e, v) => e.Name = v!,
            required: true, nullable: false, maxLength: 150),
        ContentFields.RefReq<Stigma>("providerConstellationId", "Provider constellation", EntityType.Constellation,
            e => e.ProviderConstellationId, (e, v) => e.ProviderConstellationId = v),
        ContentFields.Int<Stigma>("activationCost", "Activation cost", e => e.ActivationCost,
            (e, v) => e.ActivationCost = v, required: false),
        ContentFields.LongText<Stigma>("effect", "Effect", e => e.Effect, (e, v) => e.Effect = v!,
            required: true, nullable: false, maxLength: 4000),
    ];
}
