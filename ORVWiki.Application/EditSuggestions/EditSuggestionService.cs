using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using ORVWiki.Application.Common;
using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.EditSuggestions.Dtos;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;
using ORVWiki.Application.Notifications;
using ORVWiki.Application.Pages;

namespace ORVWiki.Application.EditSuggestions;

public class EditSuggestionService(
    IEditSuggestionRepository suggestions,
    INotificationService notifications,
    IMemoryCache cache,
    TimeProvider clock) : IEditSuggestionService
{
    public async Task<EditSuggestionDto> SubmitAsync(
        CreateEditSuggestionRequest request, long userId, CancellationToken ct = default)
    {
        if (!await suggestions.PageExistsAsync(request.PageId, ct))
            throw new NotFoundException($"Page {request.PageId} not found.");

        var doc = JsonDocument.Parse(request.ProposedChanges.GetRawText());
        var entity = new EditSuggestion
        {
            UserId = userId,
            PageId = request.PageId,
            ProposedChanges = doc,
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
            result.Items.Select(ToDto).ToList(),
            result.Total,
            result.Page,
            result.PageSize);
    }

    public async Task<PaginatedResult<EditSuggestionDto>> ListMineAsync(
        long userId, PaginationParams p, CancellationToken ct = default)
    {
        var result = await suggestions.ListByUserAsync(userId, p, ct);
        return new PaginatedResult<EditSuggestionDto>(
            result.Items.Select(ToDto).ToList(),
            result.Total,
            result.Page,
            result.PageSize);
    }

    public async Task<EditSuggestionDto> ApproveAsync(long id, long reviewerId, CancellationToken ct = default)
    {
        var s = await ReloadAsync(id, ct);
        EnsurePending(s);

        ApplyDiffToPage(s.ProposedChanges, s.Page, clock.GetUtcNow());

        s.Status = EditSuggestionStatus.Approved;
        s.ReviewedByUserId = reviewerId;
        s.ReviewedAt = clock.GetUtcNow();

        await suggestions.SaveChangesAsync(ct);
        cache.Remove(PageCacheKeys.BySlug(s.Page.Slug));

        await notifications.PublishAsync(
            s.UserId,
            NotificationType.EditApproved,
            new { suggestionId = s.Id, pageId = s.PageId, reviewedBy = reviewerId },
            ct);

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
            s.UserId,
            NotificationType.EditRejected,
            new { suggestionId = s.Id, pageId = s.PageId, reviewedBy = reviewerId },
            ct);

        return ToDto(s);
    }

    private async Task<EditSuggestion> ReloadAsync(long id, CancellationToken ct)
        => await suggestions.GetWithPageAndUsersAsync(id, ct)
            ?? throw new NotFoundException($"Edit suggestion {id} not found.");

    private static void EnsurePending(EditSuggestion s)
    {
        if (s.Status != EditSuggestionStatus.Pending)
            throw new ConflictException($"Suggestion already {s.Status.ToString().ToLowerInvariant()}.");
    }

    /// <summary>
    /// Applies the JSON diff to known Page-level fields. Unknown keys are silently
    /// ignored — they're meant to flag deeper edits the editor must apply manually.
    /// </summary>
    private static void ApplyDiffToPage(JsonDocument diff, Page page, DateTimeOffset now)
    {
        if (diff.RootElement.ValueKind != JsonValueKind.Object) return;

        foreach (var prop in diff.RootElement.EnumerateObject())
        {
            switch (prop.Name)
            {
                case "title" when prop.Value.ValueKind == JsonValueKind.String:
                    page.Title = prop.Value.GetString()!;
                    break;
                case "shortDescription":
                    page.ShortDescription = prop.Value.ValueKind == JsonValueKind.Null
                        ? null
                        : prop.Value.GetString();
                    break;
                case "discoveryChapter" when prop.Value.ValueKind == JsonValueKind.Number:
                    page.DiscoveryChapter = prop.Value.GetInt32();
                    break;
            }
        }

        page.UpdatedAt = now;
    }

    private static EditSuggestionDto ToDto(EditSuggestion s) => new(
        s.Id,
        s.UserId,
        s.User?.Username ?? "[unknown]",
        s.PageId,
        s.Page.Slug,
        s.ProposedChanges.RootElement.Clone(),
        s.Reason,
        s.Status,
        s.ReviewedByUserId,
        s.ReviewedByUser?.Username,
        s.ReviewedAt,
        s.CreatedAt);
}
