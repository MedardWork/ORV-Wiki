using ORVWiki.Application.Comments.Dtos;
using ORVWiki.Application.Common.Exceptions;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;
using ORVWiki.Application.Notifications;

namespace ORVWiki.Application.Comments;

public class CommentService(
    ICommentRepository comments,
    INotificationService notifications,
    TimeProvider clock) : ICommentService
{
    public async Task<IReadOnlyList<CommentDto>> ListVisibleByPageAsync(
        long pageId, long? viewingUserId, int currentChapter, CancellationToken ct = default)
    {
        var rows = await comments.ListVisibleByPageAsync(pageId, currentChapter, ct);
        if (rows.Count == 0) return [];

        var ids = rows.Select(c => c.Id).ToList();
        var reactions = await comments.GetReactionsForCommentsAsync(ids, ct);
        var byComment = reactions.ToLookup(r => r.CommentId);

        return rows
            .Select(c => ToDto(c, byComment[c.Id], viewingUserId))
            .ToList();
    }

    public async Task<CommentDto> CreateAsync(
        CreateCommentRequest request, long userId, int currentChapter, CancellationToken ct = default)
    {
        if (!await comments.PageExistsAsync(request.PageId, ct))
            throw new NotFoundException($"Page {request.PageId} not found.");

        if (request.ParentCommentId.HasValue)
        {
            var parent = await comments.GetParentForReplyAsync(request.ParentCommentId.Value, request.PageId, ct)
                ?? throw new NotFoundException("Parent comment not found on this page.");

            var reply = new Comment
            {
                UserId = userId,
                PageId = request.PageId,
                ParentCommentId = parent.Id,
                Body = request.Body,
                ChapterAtPost = currentChapter,
                CreatedAt = clock.GetUtcNow()
            };
            await comments.AddAsync(reply, ct);
            await comments.SaveChangesAsync(ct);

            if (parent.UserId != userId)
            {
                await notifications.PublishAsync(
                    parent.UserId,
                    NotificationType.CommentReply,
                    new { commentId = parent.Id, replyByUserId = userId, pageId = request.PageId },
                    ct);
            }

            return ToDto(await ReloadAsync(reply.Id, ct), [], userId);
        }

        var comment = new Comment
        {
            UserId = userId,
            PageId = request.PageId,
            Body = request.Body,
            ChapterAtPost = currentChapter,
            CreatedAt = clock.GetUtcNow()
        };
        await comments.AddAsync(comment, ct);
        await comments.SaveChangesAsync(ct);

        return ToDto(await ReloadAsync(comment.Id, ct), [], userId);
    }

    public async Task SoftDeleteAsync(long commentId, long userId, bool isPrivileged, CancellationToken ct = default)
    {
        var comment = await comments.GetWithUserAsync(commentId, ct)
            ?? throw new NotFoundException($"Comment {commentId} not found.");

        if (comment.UserId != userId && !isPrivileged)
            throw new ForbiddenException("You can only delete your own comments.");

        if (comment.IsDeleted) return;

        comment.IsDeleted = true;
        await comments.SaveChangesAsync(ct);
    }

    public async Task<bool> ToggleReactionAsync(
        long commentId, long userId, CommentReactionType type, CancellationToken ct = default)
    {
        var existing = await comments.GetReactionAsync(commentId, userId, type, ct);
        if (existing is not null)
        {
            comments.RemoveReaction(existing);
            await comments.SaveChangesAsync(ct);
            return false;
        }

        // Verify comment exists before inserting (avoids opaque FK violation).
        var comment = await comments.GetByIdAsync(commentId, ct)
            ?? throw new NotFoundException($"Comment {commentId} not found.");

        await comments.AddReactionAsync(new CommentReaction
        {
            CommentId = comment.Id,
            UserId = userId,
            ReactionType = type,
            CreatedAt = clock.GetUtcNow()
        }, ct);
        await comments.SaveChangesAsync(ct);
        return true;
    }

    private async Task<Comment> ReloadAsync(long id, CancellationToken ct)
        => await comments.GetWithUserAsync(id, ct)
            ?? throw new InvalidOperationException("Comment vanished after save.");

    private static CommentDto ToDto(
        Comment c, IEnumerable<CommentReaction> reactions, long? viewingUserId)
    {
        var summary = reactions
            .GroupBy(r => r.ReactionType)
            .Select(g => new ReactionSummary(
                g.Key,
                g.Count(),
                viewingUserId.HasValue && g.Any(r => r.UserId == viewingUserId.Value)))
            .ToList();

        var bodyVisible = !c.IsDeleted;
        return new CommentDto(
            c.Id,
            c.PageId,
            c.ParentCommentId,
            c.UserId,
            bodyVisible ? c.User?.Username : null,
            bodyVisible ? c.Body : null,
            c.ChapterAtPost,
            c.IsDeleted,
            c.CreatedAt,
            summary);
    }
}
