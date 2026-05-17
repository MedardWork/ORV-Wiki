using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content;

public interface IContentMutationService
{
    /// <summary>Validates a diff against a descriptor without applying it. Returns error messages.</summary>
    Task<IReadOnlyList<string>> ValidateAsync(
        SuggestionOperation operation, IContentTypeDescriptor descriptor,
        ContentDiff diff, long? pageId, CancellationToken ct = default);

    /// <summary>
    /// Validates and applies a diff to the tracked entity graph. Does NOT call
    /// SaveChanges — the caller persists. Returns the affected Page.
    /// Throws <see cref="Common.Exceptions.ValidationException"/> on invalid input.
    /// </summary>
    Task<Page> ApplyAsync(
        SuggestionOperation operation, IContentTypeDescriptor descriptor,
        ContentDiff diff, long? pageId, CancellationToken ct = default);

    /// <summary>Reads the current raw (un-rendered) field + relation values for an edit form.</summary>
    Task<ContentSnapshot> SnapshotAsync(
        IContentTypeDescriptor descriptor, long pageId, CancellationToken ct = default);
}
