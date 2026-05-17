using System.Text.Json;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.EditSuggestions.Dtos;

public record EditSuggestionDto(
    long Id,
    long UserId,
    string Username,
    SuggestionOperation Operation,
    EntityType EntityType,
    long? PageId,
    string? PageSlug,
    string? PageTitle,
    JsonElement ProposedChanges,
    string? Reason,
    EditSuggestionStatus Status,
    long? ReviewedByUserId,
    string? ReviewedByUsername,
    DateTimeOffset? ReviewedAt,
    DateTimeOffset CreatedAt);
