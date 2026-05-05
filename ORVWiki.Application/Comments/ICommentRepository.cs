using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Comments;

public interface ICommentRepository : IRepository<Comment>
{
    /// <summary>
    /// Spec rule: only comments with <c>chapter_at_post &lt;= currentChapter</c> are visible.
    /// Includes the author so callers can fill in usernames without N+1.
    /// </summary>
    Task<IReadOnlyList<Comment>> ListVisibleByPageAsync(long pageId, int currentChapter, CancellationToken ct = default);

    Task<Comment?> GetWithUserAsync(long id, CancellationToken ct = default);

    Task<bool> PageExistsAsync(long pageId, CancellationToken ct = default);
    Task<Comment?> GetParentForReplyAsync(long parentCommentId, long pageId, CancellationToken ct = default);

    Task<IReadOnlyList<CommentReaction>> GetReactionsForCommentsAsync(IEnumerable<long> commentIds, CancellationToken ct = default);
    Task<CommentReaction?> GetReactionAsync(long commentId, long userId, CommentReactionType type, CancellationToken ct = default);
    Task AddReactionAsync(CommentReaction reaction, CancellationToken ct = default);
    void RemoveReaction(CommentReaction reaction);
}
