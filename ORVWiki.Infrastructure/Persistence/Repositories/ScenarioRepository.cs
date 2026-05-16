using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Entities;

namespace ORVWiki.Infrastructure.Persistence.Repositories;

// Detail reads of a scenario eager-load the locations it plays out in, filtered
// to those the reader has already discovered so the spoiler gate holds for the
// linked pages too.
public class ScenarioRepository(AppDbContext db) : PagedEntityRepository<Scenario>(db)
{
    protected override IQueryable<Scenario> DetailQuery(int currentChapter)
        => VisibleQuery(currentChapter)
            .Include(s => s.ScenarioLocations
                    .Where(sl => sl.Location.Page.DiscoveryChapter <= currentChapter))
                .ThenInclude(sl => sl.Location)
                    .ThenInclude(l => l.Page);
}
