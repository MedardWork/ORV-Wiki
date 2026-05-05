using ORVWiki.Application.Common;
using ORVWiki.Application.EditSuggestions.Dtos;
using ORVWiki.Application.Enums;

namespace ORVWiki.Application.EditSuggestions;

public interface IEditSuggestionService
{
    Task<EditSuggestionDto> SubmitAsync(
        CreateEditSuggestionRequest request, long userId, CancellationToken ct = default);

    Task<EditSuggestionDto> GetAsync(long id, CancellationToken ct = default);

    Task<PaginatedResult<EditSuggestionDto>> ListAsync(
        EditSuggestionStatus? status, PaginationParams p, CancellationToken ct = default);

    Task<PaginatedResult<EditSuggestionDto>> ListMineAsync(
        long userId, PaginationParams p, CancellationToken ct = default);

    Task<EditSuggestionDto> ApproveAsync(long id, long reviewerId, CancellationToken ct = default);
    Task<EditSuggestionDto> RejectAsync(long id, long reviewerId, CancellationToken ct = default);
}
