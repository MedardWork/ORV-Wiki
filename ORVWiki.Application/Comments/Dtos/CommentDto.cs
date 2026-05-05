namespace ORVWiki.Application.Comments.Dtos;

public record CommentDto(
    long Id,
    long PageId,
    long? ParentCommentId,
    long UserId,
    string? Username,
    string? Body,
    int ChapterAtPost,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    IReadOnlyList<ReactionSummary> Reactions);
