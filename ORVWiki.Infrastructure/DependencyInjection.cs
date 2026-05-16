using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ORVWiki.Application.Auth;
using ORVWiki.Application.Bookmarks;
using ORVWiki.Application.Characters;
using ORVWiki.Application.Comments;
using ORVWiki.Application.Common;
using ORVWiki.Application.EditSuggestions;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Enums;
using ORVWiki.Application.Notifications;
using ORVWiki.Application.Pages;
using ORVWiki.Infrastructure.Auth;
using ORVWiki.Infrastructure.Persistence;
using ORVWiki.Infrastructure.Persistence.Repositories;

namespace ORVWiki.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        MapEnums(dataSourceBuilder);
        var dataSource = dataSourceBuilder.Build();

        services.AddSingleton(dataSource);
        services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(dataSource)
                .UseSnakeCaseNamingConvention());
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IPagedEntityRepository<>), typeof(PagedEntityRepository<>));
        // Scenario and Location override the generic paged repo to eager-load their
        // ScenarioLocation links. Closed-type registrations placed after the open
        // generic win when that specific IPagedEntityRepository<T> is resolved.
        services.AddScoped<IPagedEntityRepository<Scenario>, ScenarioRepository>();
        services.AddScoped<IPagedEntityRepository<Location>, LocationRepository>();
        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IBookmarkRepository, BookmarkRepository>();
        services.AddScoped<IEditSuggestionRepository, EditSuggestionRepository>();

        return services;
    }

    private static void MapEnums(NpgsqlDataSourceBuilder b)
    {
        b.MapEnum<EntityType>();
        b.MapEnum<CharacterStatus>();
        b.MapEnum<Gender>();
        b.MapEnum<ConstellationGrade>();
        b.MapEnum<DokkaebiRank>();
        b.MapEnum<FableGrade>();
        b.MapEnum<AttributeRarity>();
        b.MapEnum<SkillType>();
        b.MapEnum<ItemGrade>();
        b.MapEnum<ScenarioType>();
        b.MapEnum<ScenarioDifficulty>();
        b.MapEnum<EventImportance>();
        b.MapEnum<ConceptImpact>();
        b.MapEnum<CharacterConstellationRel>();
        b.MapEnum<EventCharacterRole>();
        b.MapEnum<ScenarioOutcome>();
        b.MapEnum<CommentReactionType>();
        b.MapEnum<EditSuggestionStatus>();
        b.MapEnum<NotificationType>();
    }
}
