using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Bookmarks.Dtos;

public record BookmarkDto(
    long Id,
    long PageId,
    string Slug,
    string Title,
    EntityType EntityType,
    DateTimeOffset CreatedAt);
