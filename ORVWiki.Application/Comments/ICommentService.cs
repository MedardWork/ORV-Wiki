using ORVWiki.Application.Comments.Dtos;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Comments;

public interface ICommentService
{
    Task<IReadOnlyList<CommentDto>> ListVisibleByPageAsync(
        long pageId, long? viewingUserId, int currentChapter, CancellationToken ct = default);

    Task<CommentDto> CreateAsync(
        CreateCommentRequest request, long userId, int currentChapter, CancellationToken ct = default);

    Task SoftDeleteAsync(long commentId, long userId, bool isPrivileged, CancellationToken ct = default);

    /// <summary>
    /// Toggle: returns true if the reaction was added, false if it was removed.
    /// </summary>
    Task<bool> ToggleReactionAsync(long commentId, long userId, CommentReactionType type, CancellationToken ct = default);
}
