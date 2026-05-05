using System.Text.Json;

namespace ORVWiki.Application.EditSuggestions.Dtos;

public record CreateEditSuggestionRequest(
    long PageId,
    JsonElement ProposedChanges,
    string? Reason);
