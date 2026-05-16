using ORVWiki.Application.Tags.Dtos;

namespace ORVWiki.Application.Tags;

public interface ITagService
{
    Task<IReadOnlyList<TagDto>> ListAllAsync(CancellationToken ct = default);
}
