using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using ORVWiki.Application.Content.Dtos;
using ORVWiki.Application.EditSuggestions;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;
using ORVWiki.Application.Pages;

namespace ORVWiki.Application.Content;

/// <summary>
/// Editor-facing direct content writes. The mutation is applied and an
/// auto-approved <see cref="EditSuggestion"/> log row is written in the same
/// transaction, so the suggestion table doubles as the wiki's change history.
/// </summary>
public sealed class EditorContentService(
    IContentMutationService mutations,
    IContentTypeRegistry registry,
    IEditSuggestionRepository suggestions,
    IMemoryCache cache,
    TimeProvider clock) : IEditorContentService
{
    public async Task<ContentWriteResult> CreateAsync(
        EntityType type, ContentWriteRequest request, long editorUserId, CancellationToken ct = default)
    {
        var descriptor = registry.Get(type);
        var diff = ContentDiff.Parse(request.Changes);
        var page = await mutations.ApplyAsync(SuggestionOperation.Create, descriptor, diff, null, ct);

        await suggestions.AddAsync(
            BuildLog(SuggestionOperation.Create, type, page, request.Changes, request.Reason, editorUserId, linkPage: true),
            ct);
        await suggestions.SaveChangesAsync(ct);

        InvalidateCache(page);
        return new ContentWriteResult(page.Id, page.Slug, page.Title, type);
    }

    public async Task<ContentWriteResult> UpdateAsync(
        EntityType type, long pageId, ContentWriteRequest request, long editorUserId, CancellationToken ct = default)
    {
        var descriptor = registry.Get(type);
        var diff = ContentDiff.Parse(request.Changes);
        var page = await mutations.ApplyAsync(SuggestionOperation.Update, descriptor, diff, pageId, ct);

        await suggestions.AddAsync(
            BuildLog(SuggestionOperation.Update, type, page, request.Changes, request.Reason, editorUserId, linkPage: true),
            ct);
        await suggestions.SaveChangesAsync(ct);

        InvalidateCache(page);
        return new ContentWriteResult(page.Id, page.Slug, page.Title, type);
    }

    public async Task DeleteAsync(
        EntityType type, long pageId, long editorUserId, CancellationToken ct = default)
    {
        var descriptor = registry.Get(type);
        var page = await mutations.ApplyAsync(SuggestionOperation.Delete, descriptor, new ContentDiff(), pageId, ct);

        // The page row is removed in this same transaction, so the log row cannot
        // reference it by FK — record what was deleted in the payload instead.
        var deletedSnapshot = JsonSerializer.SerializeToElement(new
        {
            fields = new { slug = page.Slug, title = page.Title }
        });
        await suggestions.AddAsync(
            BuildLog(SuggestionOperation.Delete, type, page, deletedSnapshot, reason: null, editorUserId, linkPage: false),
            ct);
        await suggestions.SaveChangesAsync(ct);

        InvalidateCache(page);
    }

    public Task<ContentSnapshot> GetForEditAsync(EntityType type, long pageId, CancellationToken ct = default)
        => mutations.SnapshotAsync(registry.Get(type), pageId, ct);

    private EditSuggestion BuildLog(
        SuggestionOperation operation, EntityType type, Page page,
        JsonElement changes, string? reason, long editorUserId, bool linkPage)
    {
        var now = clock.GetUtcNow();
        var log = new EditSuggestion
        {
            UserId = editorUserId,
            Operation = operation,
            EntityType = type,
            ProposedChanges = JsonDocument.Parse(changes.GetRawText()),
            Reason = reason,
            Status = EditSuggestionStatus.Approved,
            ReviewedByUserId = editorUserId,
            ReviewedAt = now,
            CreatedAt = now
        };
        if (linkPage) log.Page = page;
        return log;
    }

    private void InvalidateCache(Page page)
    {
        if (!string.IsNullOrEmpty(page.Slug))
            cache.Remove(PageCacheKeys.BySlug(page.Slug));
    }
}
