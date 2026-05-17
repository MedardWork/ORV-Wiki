using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ORVWiki.Application.Arcs;
using ORVWiki.Application.Attributes;
using ORVWiki.Application.Auth;
using ORVWiki.Application.Bookmarks;
using ORVWiki.Application.Characters;
using ORVWiki.Application.Comments;
using ORVWiki.Application.Concepts;
using ORVWiki.Application.Constellations;
using ORVWiki.Application.Content;
using ORVWiki.Application.DemonKings;
using ORVWiki.Application.Dokkaebis;
using ORVWiki.Application.EditSuggestions;
using ORVWiki.Application.Events;
using ORVWiki.Application.Fables;
using ORVWiki.Application.Items;
using ORVWiki.Application.Locations;
using ORVWiki.Application.Nebulae;
using ORVWiki.Application.Notifications;
using ORVWiki.Application.OuterGods;
using ORVWiki.Application.Pages;
using ORVWiki.Application.Scenarios;
using ORVWiki.Application.Skills;
using ORVWiki.Application.Spoilers;
using ORVWiki.Application.Stigmas;
using ORVWiki.Application.Tags;
using ORVWiki.Application.Timeline;
using ORVWiki.Application.Worldlines;

namespace ORVWiki.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<ISpoilerService, SpoilerService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPageService, PageService>();
        services.AddScoped<ICharacterService, CharacterService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IBookmarkService, BookmarkService>();
        services.AddScoped<IEditSuggestionService, EditSuggestionService>();
        services.AddScoped<ITimelineService, TimelineService>();
        services.AddScoped<ITagService, TagService>();

        // Spoiler-aware read services for the rest of the encyclopedic entities.
        services.AddScoped<ArcService>();
        services.AddScoped<AttributeService>();
        services.AddScoped<ConceptService>();
        services.AddScoped<ConstellationService>();
        services.AddScoped<DemonKingService>();
        services.AddScoped<DokkaebiService>();
        services.AddScoped<EventService>();
        services.AddScoped<FableService>();
        services.AddScoped<ItemService>();
        services.AddScoped<LocationService>();
        services.AddScoped<NebulaService>();
        services.AddScoped<OuterGodService>();
        services.AddScoped<ScenarioService>();
        services.AddScoped<SkillService>();
        services.AddScoped<StigmaService>();
        services.AddScoped<WorldlineService>();

        // Generic content-management engine + per-type descriptors (assembly-scanned).
        foreach (var descriptorType in typeof(DependencyInjection).Assembly.GetTypes()
                     .Where(t => t is { IsAbstract: false, IsInterface: false }
                                 && typeof(IContentTypeDescriptor).IsAssignableFrom(t)))
            services.AddSingleton(typeof(IContentTypeDescriptor), descriptorType);
        services.AddSingleton<IContentTypeRegistry, ContentTypeRegistry>();
        services.AddScoped<IContentMutationService, ContentMutationService>();
        services.AddScoped<IEditorContentService, EditorContentService>();

        services.AddValidatorsFromAssemblyContaining<AuthService>();
        return services;
    }
}
