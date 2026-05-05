using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;

namespace ORVWiki.Application.Characters;

public interface ICharacterRepository : IRepository<Character>
{
    Task<Character?> GetWithPageByIdAsync(long id, CancellationToken ct = default);
    Task<Character?> GetVisibleBySlugAsync(string slug, int currentChapter, CancellationToken ct = default);
    Task<Character?> GetVisibleByIdAsync(long id, int currentChapter, CancellationToken ct = default);

    Task<PaginatedResult<Character>> ListVisibleAsync(
        int currentChapter,
        PaginationParams pagination,
        CancellationToken ct = default);

    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
}
