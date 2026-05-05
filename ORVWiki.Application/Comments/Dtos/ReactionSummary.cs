using ORVWiki.Application.Enums;

namespace ORVWiki.Application.Comments.Dtos;

public record ReactionSummary(CommentReactionType Type, int Count, bool MyReaction);
