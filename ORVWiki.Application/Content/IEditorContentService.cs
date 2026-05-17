using ORVWiki.Application.Content.Dtos;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Content;

/// <summary>
/// Editor-facing direct content writes. Each write applies immediately and is
/// recorded as an auto-approved <c>EditSuggestion</c> for the change history.
/// </summary>
public interface IEditorContentService
{
    Task<ContentWriteResult> CreateAsync(
        EntityType type, ContentWriteRequest request, long editorUserId, CancellationToken ct = default);

    Task<ContentWriteResult> UpdateAsync(
        EntityType type, long pageId, ContentWriteRequest request, long editorUserId, CancellationToken ct = default);

    Task DeleteAsync(EntityType type, long pageId, long editorUserId, CancellationToken ct = default);

    Task<ContentSnapshot> GetForEditAsync(EntityType type, long pageId, CancellationToken ct = default);
}
