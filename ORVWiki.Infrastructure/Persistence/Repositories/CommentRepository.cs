using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Comments;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Infrastructure.Persistence.Repositories;

public class CommentRepository(AppDbContext db)
    : Repository<Comment>(db), ICommentRepository
{
    public async Task<IReadOnlyList<Comment>> ListVisibleByPageAsync(
        long pageId, int currentChapter, CancellationToken ct = default)
        => await Db.Comments
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.PageId == pageId && c.ChapterAtPost <= currentChapter)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

    public Task<Comment?> GetWithUserAsync(long id, CancellationToken ct = default)
        => Db.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> PageExistsAsync(long pageId, CancellationToken ct = default)
        => Db.Pages.AnyAsync(p => p.Id == pageId, ct);

    public Task<Comment?> GetParentForReplyAsync(long parentCommentId, long pageId, CancellationToken ct = default)
        => Db.Comments.FirstOrDefaultAsync(c => c.Id == parentCommentId && c.PageId == pageId, ct);

    public async Task<IReadOnlyList<CommentReaction>> GetReactionsForCommentsAsync(
        IEnumerable<long> commentIds, CancellationToken ct = default)
    {
        var ids = commentIds.ToArray();
        return await Db.CommentReactions
            .AsNoTracking()
            .Where(r => ids.Contains(r.CommentId))
            .ToListAsync(ct);
    }

    public Task<CommentReaction?> GetReactionAsync(
        long commentId, long userId, CommentReactionType type, CancellationToken ct = default)
        => Db.CommentReactions
            .FirstOrDefaultAsync(r =>
                r.CommentId == commentId && r.UserId == userId && r.ReactionType == type, ct);

    public Task AddReactionAsync(CommentReaction reaction, CancellationToken ct = default)
        => Db.CommentReactions.AddAsync(reaction, ct).AsTask();

    public void RemoveReaction(CommentReaction reaction)
        => Db.CommentReactions.Remove(reaction);
}
