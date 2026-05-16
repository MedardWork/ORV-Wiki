using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Entities;

namespace ORVWiki.Infrastructure.Persistence.Repositories;

// Detail reads of a location eager-load the scenarios staged there, filtered to
// those the reader has already discovered so the spoiler gate holds for the
// linked pages too.
public class LocationRepository(AppDbContext db) : PagedEntityRepository<Location>(db)
{
    protected override IQueryable<Location> DetailQuery(int currentChapter)
        => VisibleQuery(currentChapter)
            .Include(l => l.ScenarioLocations
                    .Where(sl => sl.Scenario.Page.DiscoveryChapter <= currentChapter))
                .ThenInclude(sl => sl.Scenario)
                    .ThenInclude(s => s.Page);
}
