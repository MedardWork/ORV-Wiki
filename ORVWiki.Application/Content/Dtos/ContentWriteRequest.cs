using System.Text.Json;

namespace ORVWiki.Application.Content.Dtos;

/// <summary>Body of a direct editor create/update: the content diff plus an optional note.</summary>
public record ContentWriteRequest(JsonElement Changes, string? Reason);
