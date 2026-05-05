using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Entities.Pivots;
using AttributeEntity = ORVWiki.Application.Entities.Attribute;

namespace ORVWiki.Application.Common;

public interface IAppDbContext
{
    DbSet<Page> Pages { get; }
    DbSet<Character> Characters { get; }
    DbSet<Constellation> Constellations { get; }
    DbSet<Nebula> Nebulae { get; }
    DbSet<Dokkaebi> Dokkaebi { get; }
    DbSet<DemonKing> DemonKings { get; }
    DbSet<OuterGod> OuterGods { get; }
    DbSet<Worldline> Worldlines { get; }
    DbSet<Fable> Fables { get; }
    DbSet<Stigma> Stigmas { get; }
    DbSet<AttributeEntity> Attributes { get; }
    DbSet<Skill> Skills { get; }
    DbSet<Item> Items { get; }
    DbSet<Location> Locations { get; }
    DbSet<Scenario> Scenarios { get; }
    DbSet<Arc> Arcs { get; }
    DbSet<Chapter> Chapters { get; }
    DbSet<Event> Events { get; }
    DbSet<Concept> Concepts { get; }
    DbSet<Role> Roles { get; }
    DbSet<User> Users { get; }
    DbSet<Comment> Comments { get; }
    DbSet<CommentReaction> CommentReactions { get; }
    DbSet<EditSuggestion> EditSuggestions { get; }
    DbSet<Bookmark> Bookmarks { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Media> Media { get; }
    DbSet<Tag> Tags { get; }
    DbSet<CharacterStigma> CharacterStigmas { get; }
    DbSet<CharacterAttribute> CharacterAttributes { get; }
    DbSet<CharacterSkill> CharacterSkills { get; }
    DbSet<CharacterItem> CharacterItems { get; }
    DbSet<CharacterFable> CharacterFables { get; }
    DbSet<CharacterConstellation> CharacterConstellations { get; }
    DbSet<EventCharacter> EventCharacters { get; }
    DbSet<ScenarioParticipant> ScenarioParticipants { get; }
    DbSet<ScenarioLocation> ScenarioLocations { get; }
    DbSet<PageTag> PageTags { get; }
    DbSet<EventConnection> EventConnections { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
