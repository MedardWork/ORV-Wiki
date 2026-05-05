using ORVWiki.Application.Common;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Entities;

public class Dokkaebi : IPagedEntity
{
    public long Id { get; set; }
    public long PageId { get; set; }
    public Page Page { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? ChannelId { get; set; }
    public DokkaebiRank Rank { get; set; } = DokkaebiRank.Low;
    public string? Speciality { get; set; }
}
