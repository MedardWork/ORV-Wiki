using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ORVWiki.Application.Auth;
using ORVWiki.Application.Entities;
using ORVWiki.Infrastructure.Persistence.Seed;

namespace ORVWiki.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(ct);
        await SeedRolesAsync(db, ct);
        await Arc01_ThreeWaysToSurvive.SeedAsync(db, ct);
    }

    private static async Task SeedRolesAsync(AppDbContext db, CancellationToken ct)
    {
        var existing = await db.Roles.Select(r => r.Name).ToListAsync(ct);
        var missing = Roles.All.Where(r => !existing.Contains(r)).ToList();
        if (missing.Count == 0) return;

        foreach (var name in missing)
            db.Roles.Add(new Role { Name = name });

        await db.SaveChangesAsync(ct);
    }
}
