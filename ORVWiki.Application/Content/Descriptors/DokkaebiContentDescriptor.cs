using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Descriptors;

public sealed class DokkaebiContentDescriptor : ContentTypeDescriptor<Dokkaebi>
{
    public override EntityType EntityType => EntityType.Dokkaebi;
    public override string DisplayName => "Dokkaebi";
    protected override DbSet<Dokkaebi> Set(IAppDbContext db) => db.Dokkaebi;

    public override IReadOnlyList<ContentField> Fields { get; } =
    [
        .. PageFields.All<Dokkaebi>(),
        ContentFields.Text<Dokkaebi>("name", "Name", e => e.Name, (e, v) => e.Name = v!,
            required: true, nullable: false, maxLength: 150),
        ContentFields.Text<Dokkaebi>("channelId", "Channel ID", e => e.ChannelId, (e, v) => e.ChannelId = v,
            maxLength: 80),
        ContentFields.EnumOf<Dokkaebi, DokkaebiRank>("rank", "Rank", e => e.Rank, (e, v) => e.Rank = v),
        ContentFields.Text<Dokkaebi>("speciality", "Speciality", e => e.Speciality, (e, v) => e.Speciality = v,
            maxLength: 200),
    ];
}
