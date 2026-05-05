using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Skills.Dtos;
using ORVWiki.Application.Spoilers;

namespace ORVWiki.Application.Skills;

public class SkillService(
    IPagedEntityRepository<Skill> repository,
    ISpoilerService spoilers)
    : PagedEntityReadService<Skill, SkillDto, SkillListItemDto>(repository, spoilers)
{
    protected override string EntityName => "Skill";

    protected override SkillDto ToDto(Skill s, int currentChapter) => new(
        s.Id,
        s.PageId,
        s.Page.Slug,
        Spoilers.RenderInline(s.Page.Title, currentChapter),
        s.Page.DiscoveryChapter,
        Spoilers.RenderInline(s.Page.ShortDescription, currentChapter),
        s.Name,
        s.SkillType,
        s.CostInCoins,
        s.MaxLevel,
        Spoilers.RenderInline(s.Effect, currentChapter));

    protected override SkillListItemDto ToListItem(Skill s) => new(
        s.Id,
        s.Page.Slug,
        s.Name,
        s.SkillType,
        s.Page.DiscoveryChapter);
}
