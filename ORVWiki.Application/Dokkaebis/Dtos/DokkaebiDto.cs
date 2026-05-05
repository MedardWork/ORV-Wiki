using ORVWiki.Application.Enums;
using ORVWiki.Application.Spoilers.Dtos;

namespace ORVWiki.Application.Dokkaebis.Dtos;

public record DokkaebiDto(
    long Id,
    long PageId,
    string Slug,
    RenderedContent Title,
    int DiscoveryChapter,
    RenderedContent ShortDescription,
    string Name,
    string? ChannelId,
    DokkaebiRank Rank,
    RenderedContent Speciality);
