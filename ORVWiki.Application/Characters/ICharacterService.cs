using ORVWiki.Application.Characters.Dtos;
using ORVWiki.Application.Common;

namespace ORVWiki.Application.Characters;

public interface ICharacterService
{
    Task<CharacterDetailDto> GetVisibleByIdAsync(long id, int currentChapter, CancellationToken ct = default);
    Task<CharacterDetailDto> GetVisibleBySlugAsync(string slug, int currentChapter, CancellationToken ct = default);

    Task<PaginatedResult<CharacterListItemDto>> ListVisibleAsync(
        PaginationParams pagination,
        int currentChapter,
        CancellationToken ct = default);
}
