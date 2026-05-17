using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.EditSuggestions;

public interface IEditSuggestionRepository : IRepository<EditSuggestion>
{
    Task<EditSuggestion?> GetWithPageAndUsersAsync(long id, CancellationToken ct = default);

    Task<PaginatedResult<EditSuggestion>> ListAsync(
        EditSuggestionStatus? status, PaginationParams p, CancellationToken ct = default);

    Task<PaginatedResult<EditSuggestion>> ListByUserAsync(
        long userId, PaginationParams p, CancellationToken ct = default);

    Task<EntityType?> GetPageEntityTypeAsync(long pageId, CancellationToken ct = default);
}
