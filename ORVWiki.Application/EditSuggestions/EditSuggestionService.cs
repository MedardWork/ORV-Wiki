using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using ORVWiki.Application.Common;
using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.Content;
using ORVWiki.Application.EditSuggestions.Dtos;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;
using ORVWiki.Application.Notifications;
using ORVWiki.Application.Pages;

namespace ORVWiki.Application.EditSuggestions;

public class EditSuggestionService(
    IEditSuggestionRepository suggestions,
    IContentMutationService mutations,
    IContentTypeRegistry registry,
    INotificationService notifications,
    IMemoryCache cache,
    TimeProvider clock) : IEditSuggestionService
{
    public async Task<EditSuggestionDto> SubmitAsync(
        CreateEditSuggestionRequest request, long userId, CancellationToken ct = default)
    {
        if (request.Operation == SuggestionOperation.Delete)
            throw new ForbiddenException("Deletions cannot be suggested — ask an editor.");

        var entityType = await ResolveEntityTypeAsync(
            request.Operation, request.EntityType, request.PageId, ct);
        var descriptor = registry.Get(entityType);
        var diff = ContentDiff.Parse(request.ProposedChanges);

        if (diff.IsEmpty)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["proposedChanges"] = ["The suggestion contains no changes."]
            });

        var errors = await mutations.ValidateAsync(request.Operation, descriptor, diff, request.PageId, ct);
        if (errors.Count > 0)
            throw new ValidationException(new Dictionary<string, string[]> { ["content"] = [.. errors] });

        var entity = new EditSuggestion
        {
            UserId = userId,
            Operation = request.Operation,
            EntityType = entityType,
            PageId = request.Operation == SuggestionOperation.Create ? null : request.PageId,
            ProposedChanges = JsonDocument.Parse(request.ProposedChanges.GetRawText()),
            Reason = request.Reason,
            Status = EditSuggestionStatus.Pending,
            CreatedAt = clock.GetUtcNow()
        };

        await suggestions.AddAsync(entity, ct);
        await suggestions.SaveChangesAsync(ct);

        return ToDto(await ReloadAsync(entity.Id, ct));
    }

    public async Task<EditSuggestionDto> GetAsync(long id, CancellationToken ct = default)
        => ToDto(await ReloadAsync(id, ct));

    public async Task<PaginatedResult<EditSuggestionDto>> ListAsync(
        EditSuggestionStatus? status, PaginationParams p, CancellationToken ct = default)
    {
        var result = await suggestions.ListAsync(status, p, ct);
        return new PaginatedResult<EditSuggestionDto>(
            result.Items.Select(ToDto).ToList(), result.Total, result.Page, result.PageSize);
    }

    public async Task<PaginatedResult<EditSuggestionDto>> ListMineAsync(
        long userId, PaginationParams p, CancellationToken ct = default)
    {
        var result = await suggestions.ListByUserAsync(userId, p, ct);
        return new PaginatedResult<EditSuggestionDto>(
            result.Items.Select(ToDto).ToList(), result.Total, result.Page, result.PageSize);
    }

    public async Task<EditSuggestionDto> ApproveAsync(long id, long reviewerId, CancellationToken ct = default)
    {
        var s = await ReloadAsync(id, ct);
        EnsurePending(s);

        var descriptor = registry.Get(s.EntityType);
        var diff = ContentDiff.Parse(s.ProposedChanges.RootElement);
        var page = await mutations.ApplyAsync(s.Operation, descriptor, diff, s.PageId, ct);

        s.Status = EditSuggestionStatus.Approved;
        s.ReviewedByUserId = reviewerId;
        s.ReviewedAt = clock.GetUtcNow();
        if (s.Operation == SuggestionOperation.Create)
            s.Page = page;

        await suggestions.SaveChangesAsync(ct);

        if (!string.IsNullOrEmpty(page.Slug))
            cache.Remove(PageCacheKeys.BySlug(page.Slug));

        await notifications.PublishAsync(
            s.UserId, NotificationType.EditApproved,
            new { suggestionId = s.Id, pageId = s.PageId, reviewedBy = reviewerId }, ct);

        return ToDto(s);
    }

    public async Task<EditSuggestionDto> RejectAsync(long id, long reviewerId, CancellationToken ct = default)
    {
        var s = await ReloadAsync(id, ct);
        EnsurePending(s);

        s.Status = EditSuggestionStatus.Rejected;
        s.ReviewedByUserId = reviewerId;
        s.ReviewedAt = clock.GetUtcNow();

        await suggestions.SaveChangesAsync(ct);

        await notifications.PublishAsync(
            s.UserId, NotificationType.EditRejected,
            new { suggestionId = s.Id, pageId = s.PageId, reviewedBy = reviewerId }, ct);

        return ToDto(s);
    }

    public async Task DeleteOwnAsync(long id, long userId, CancellationToken ct = default)
    {
        var s = await ReloadAsync(id, ct);
        if (s.UserId != userId)
            throw new ForbiddenException("You can only delete your own suggestions.");

        suggestions.Remove(s);
        await suggestions.SaveChangesAsync(ct);
    }

    private async Task<EntityType> ResolveEntityTypeAsync(
        SuggestionOperation operation, EntityType requested, long? pageId, CancellationToken ct)
    {
        if (operation == SuggestionOperation.Create)
            return requested;

        if (pageId is null)
            throw new NotFoundException("A target page is required.");

        return await suggestions.GetPageEntityTypeAsync(pageId.Value, ct)
            ?? throw new NotFoundException($"Page {pageId} not found.");
    }

    private async Task<EditSuggestion> ReloadAsync(long id, CancellationToken ct)
        => await suggestions.GetWithPageAndUsersAsync(id, ct)
            ?? throw new NotFoundException($"Edit suggestion {id} not found.");

    private static void EnsurePending(EditSuggestion s)
    {
        if (s.Status != EditSuggestionStatus.Pending)
            throw new ConflictException($"Suggestion already {s.Status.ToString().ToLowerInvariant()}.");
    }

    private static EditSuggestionDto ToDto(EditSuggestion s) => new(
        s.Id,
        s.UserId,
        s.User?.Username ?? "[unknown]",
        s.Operation,
        s.EntityType,
        s.PageId,
        s.Page?.Slug,
        s.Page?.Title,
        s.ProposedChanges.RootElement.Clone(),
        s.Reason,
        s.Status,
        s.ReviewedByUserId,
        s.ReviewedByUser?.Username,
        s.ReviewedAt,
        s.CreatedAt);
}
