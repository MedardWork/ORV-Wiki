using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.Enums;

namespace ORVWiki.API.Controllers;

/// <summary>Parses a snake_case content-type route segment into an <see cref="EntityType"/>.</summary>
internal static class ContentTypeRouting
{
    public static EntityType Parse(string raw)
        => Enum.TryParse<EntityType>(raw.Replace("_", string.Empty), ignoreCase: true, out var type)
            ? type
            : throw new NotFoundException($"Unknown content type '{raw}'.");
}
