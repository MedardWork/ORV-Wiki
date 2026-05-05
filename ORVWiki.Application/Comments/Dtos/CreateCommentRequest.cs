namespace ORVWiki.Application.Comments.Dtos;

public record CreateCommentRequest(long PageId, long? ParentCommentId, string Body);
