using System.Text.Json;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.EditSuggestions.Dtos;

public record CreateEditSuggestionRequest(
    SuggestionOperation Operation,
    EntityType EntityType,
    long? PageId,
    JsonElement ProposedChanges,
    string? Reason);
