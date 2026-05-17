using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content.Dtos;

public record ContentWriteResult(long PageId, string Slug, string Title, EntityType EntityType);
