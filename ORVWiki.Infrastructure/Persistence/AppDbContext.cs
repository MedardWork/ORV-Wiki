using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Common;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;
using AttributeEntity = ORVWiki.Application.Entities.Attribute;

namespace ORVWiki.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Shared metadata
    public DbSet<Page> Pages => Set<Page>();

    // A) Beings and powers
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Constellation> Constellations => Set<Constellation>();
    public DbSet<Nebula> Nebulae => Set<Nebula>();
    public DbSet<Dokkaebi> Dokkaebi => Set<Dokkaebi>();
    public DbSet<DemonKing> DemonKings => Set<DemonKing>();
    public DbSet<OuterGod> OuterGods => Set<OuterGod>();
    public DbSet<Worldline> Worldlines => Set<Worldline>();

    // B) Power system
    public DbSet<Fable> Fables => Set<Fable>();
    public DbSet<Stigma> Stigmas => Set<Stigma>();
    public DbSet<AttributeEntity> Attributes => Set<AttributeEntity>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Item> Items => Set<Item>();

    // C) Story and world
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<Arc> Arcs => Set<Arc>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Concept> Concepts => Set<Concept>();

    // D) Community
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentReaction> CommentReactions => Set<CommentReaction>();
    public DbSet<EditSuggestion> EditSuggestions => Set<EditSuggestion>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // E) Helpers
    public DbSet<Media> Media => Set<Media>();
    public DbSet<Tag> Tags => Set<Tag>();

    // Pivots
    public DbSet<CharacterStigma> CharacterStigmas => Set<CharacterStigma>();
    public DbSet<CharacterAttribute> CharacterAttributes => Set<CharacterAttribute>();
    public DbSet<CharacterSkill> CharacterSkills => Set<CharacterSkill>();
    public DbSet<CharacterItem> CharacterItems => Set<CharacterItem>();
    public DbSet<CharacterFable> CharacterFables => Set<CharacterFable>();
    public DbSet<CharacterConstellation> CharacterConstellations => Set<CharacterConstellation>();
    public DbSet<EventCharacter> EventCharacters => Set<EventCharacter>();
    public DbSet<ScenarioParticipant> ScenarioParticipants => Set<ScenarioParticipant>();
    public DbSet<ScenarioLocation> ScenarioLocations => Set<ScenarioLocation>();
    public DbSet<PageTag> PageTags => Set<PageTag>();
    public DbSet<Jump> Jumps => Set<Jump>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        RegisterEnums(modelBuilder);
        ConfigurePage(modelBuilder);
        ConfigureBeings(modelBuilder);
        ConfigurePowerSystem(modelBuilder);
        ConfigureStoryAndWorld(modelBuilder);
        ConfigureCommunity(modelBuilder);
        ConfigureHelpers(modelBuilder);
        ConfigurePivots(modelBuilder);
    }

    private static void RegisterEnums(ModelBuilder b)
    {
        b.HasPostgresEnum<EntityType>();
        b.HasPostgresEnum<CharacterStatus>();
        b.HasPostgresEnum<Gender>();
        b.HasPostgresEnum<ConstellationGrade>();
        b.HasPostgresEnum<DokkaebiRank>();
        b.HasPostgresEnum<FableGrade>();
        b.HasPostgresEnum<AttributeRarity>();
        b.HasPostgresEnum<SkillType>();
        b.HasPostgresEnum<ItemGrade>();
        b.HasPostgresEnum<ScenarioType>();
        b.HasPostgresEnum<ScenarioDifficulty>();
        b.HasPostgresEnum<EventImportance>();
        b.HasPostgresEnum<ConceptImpact>();
        b.HasPostgresEnum<CharacterConstellationRel>();
        b.HasPostgresEnum<EventCharacterRole>();
        b.HasPostgresEnum<ScenarioOutcome>();
        b.HasPostgresEnum<CommentReactionType>();
        b.HasPostgresEnum<EditSuggestionStatus>();
        b.HasPostgresEnum<NotificationType>();
    }

    private static void ConfigurePage(ModelBuilder b)
    {
        b.Entity<Page>(e =>
        {
            e.Property(x => x.Slug).HasMaxLength(120).IsRequired();
            e.Property(x => x.Title).HasMaxLength(255).IsRequired();
            e.Property(x => x.DiscoveryChapter).HasDefaultValue(1);
            e.Property(x => x.SpoilerMap).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
            e.Property(x => x.ShortDescription).HasColumnType("text");
            e.Property(x => x.ViewCount).HasDefaultValue(0);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

            e.HasIndex(x => x.Slug).IsUnique();

            e.ToTable(t => t.HasCheckConstraint("ck_page_discovery_chapter", "discovery_chapter >= 1"));
        });
    }

    private static void ConfigureBeings(ModelBuilder b)
    {
        b.Entity<Character>(e =>
        {
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Alias).HasMaxLength(150);
            e.Property(x => x.Species).HasMaxLength(50).HasDefaultValue("human");
            e.Property(x => x.Status).HasDefaultValue(CharacterStatus.Unknown);
            e.Property(x => x.Biography).HasColumnType("text");

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Character>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();

            e.HasOne(x => x.PortraitMedia)
                .WithMany(m => m.CharacterPortraits)
                .HasForeignKey(x => x.PortraitMediaId)
                .OnDelete(DeleteBehavior.SetNull);

            e.ToTable(t => t.HasCheckConstraint(
                "ck_character_death_after_birth",
                "death_chapter IS NULL OR birth_chapter IS NULL OR death_chapter >= birth_chapter"));
        });

        b.Entity<Constellation>(e =>
        {
            e.Property(x => x.Modifier).HasMaxLength(200).IsRequired();
            e.Property(x => x.TrueName).HasMaxLength(150);
            e.Property(x => x.Description).HasColumnType("text");

            e.HasIndex(x => x.Modifier).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Constellation>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();

            e.HasOne(x => x.Nebula)
                .WithMany(n => n.Constellations)
                .HasForeignKey(x => x.NebulaId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.OriginCharacter)
                .WithMany(c => c.DeifiedConstellations)
                .HasForeignKey(x => x.OriginCharacterId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Nebula>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasColumnType("text");
            e.HasIndex(x => x.Name).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Nebula>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();

            e.HasOne(x => x.FounderConstellation)
                .WithMany()
                .HasForeignKey(x => x.FounderConstellationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Dokkaebi>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.ChannelId).HasMaxLength(20);
            e.Property(x => x.Rank).HasDefaultValue(DokkaebiRank.Low);
            e.Property(x => x.Speciality).HasColumnType("text");

            e.HasIndex(x => x.ChannelId).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Dokkaebi>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();
        });

        b.Entity<DemonKing>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.DemonRealm).HasMaxLength(100);
            e.Property(x => x.Description).HasColumnType("text");

            e.HasIndex(x => x.Ranking).IsUnique();
            e.HasIndex(x => x.Name).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<DemonKing>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();

            e.ToTable(t => t.HasCheckConstraint("ck_demon_king_ranking", "ranking BETWEEN 1 AND 72"));
        });

        b.Entity<OuterGod>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.GodType).HasMaxLength(50);
            e.Property(x => x.Description).HasColumnType("text");
            e.HasIndex(x => x.Name).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<OuterGod>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();
        });

        b.Entity<Worldline>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Description).HasColumnType("text");
            e.Property(x => x.Color).HasMaxLength(7);
            e.Property(x => x.DisplayOrder).HasDefaultValue(0);
            e.HasIndex(x => x.LineNumber).IsUnique();
            e.HasIndex(x => x.Color).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Worldline>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();

            e.HasOne(x => x.ParentWorldline)
                .WithMany(w => w.ChildWorldlines)
                .HasForeignKey(x => x.ParentWorldlineId)
                .OnDelete(DeleteBehavior.SetNull);

            e.ToTable(t => t.HasCheckConstraint(
                "ck_worldline_color_hex",
                "color IS NULL OR color ~ '^#[0-9A-Fa-f]{6}$'"));
        });
    }

    private static void ConfigurePowerSystem(ModelBuilder b)
    {
        b.Entity<Fable>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Legend).HasColumnType("text");
            e.HasIndex(x => x.Title).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Fable>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();

            e.HasOne(x => x.OriginCharacter)
                .WithMany(c => c.OriginatedFables)
                .HasForeignKey(x => x.OriginCharacterId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Stigma>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Effect).HasColumnType("text").IsRequired();
            e.Property(x => x.ActivationCost).HasDefaultValue(0);
            e.HasIndex(x => x.Name).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Stigma>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();

            e.HasOne(x => x.ProviderConstellation)
                .WithMany(c => c.ProvidedStigmas)
                .HasForeignKey(x => x.ProviderConstellationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<AttributeEntity>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Rarity).HasDefaultValue(AttributeRarity.Common);
            e.Property(x => x.Effect).HasColumnType("text");
            e.HasIndex(x => x.Name).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<AttributeEntity>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();
        });

        b.Entity<Skill>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.MaxLevel).HasDefaultValue((short)10);
            e.Property(x => x.Effect).HasColumnType("text");
            e.HasIndex(x => x.Name).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Skill>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();
        });

        b.Entity<Item>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.IsStarRelic).HasDefaultValue(false);
            e.Property(x => x.Description).HasColumnType("text");
            e.HasIndex(x => x.Name).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Item>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();
        });
    }

    private static void ConfigureStoryAndWorld(ModelBuilder b)
    {
        b.Entity<Location>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Dimension).HasMaxLength(100);
            e.Property(x => x.Coordinates).HasColumnType("jsonb");
            e.Property(x => x.Description).HasColumnType("text");

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Location>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();

            e.HasOne(x => x.Worldline)
                .WithMany(w => w.Locations)
                .HasForeignKey(x => x.WorldlineId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.ParentLocation)
                .WithMany(l => l.ChildLocations)
                .HasForeignKey(x => x.ParentLocationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Scenario>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Conditions).HasColumnType("text");
            e.Property(x => x.Rewards).HasColumnType("text");
            e.Property(x => x.Penalty).HasColumnType("text");
            e.HasIndex(x => x.Code).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Scenario>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();

            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_scenario_chapter_start", "chapter_start >= 1");
                t.HasCheckConstraint("ck_scenario_chapter_range",
                    "chapter_end IS NULL OR chapter_end >= chapter_start");
            });
        });

        b.Entity<Arc>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Summary).HasColumnType("text");
            e.HasIndex(x => x.Name).IsUnique();
            e.HasIndex(x => x.OrderNumber).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Arc>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();

            e.ToTable(t => t.HasCheckConstraint(
                "ck_arc_chapter_range",
                "chapter_end >= chapter_start AND chapter_start >= 1"));
        });

        b.Entity<Chapter>(e =>
        {
            e.HasKey(x => x.ChapterNumber);
            e.Property(x => x.ChapterNumber).ValueGeneratedNever();
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Summary).HasColumnType("text");

            e.HasOne(x => x.Arc)
                .WithMany(a => a.Chapters)
                .HasForeignKey(x => x.ArcId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Event>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasColumnType("text");
            e.Property(x => x.Importance).HasDefaultValue(EventImportance.Minor);
            e.Property(x => x.LengthEstimate).HasColumnType("text");

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Event>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();

            e.HasOne(x => x.Chapter)
                .WithMany(c => c.Events)
                .HasForeignKey(x => x.ChapterNumber)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Location)
                .WithMany(l => l.Events)
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.Worldline)
                .WithMany(w => w.Events)
                .HasForeignKey(x => x.WorldlineId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Concept>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Definition).HasColumnType("text").IsRequired();
            e.Property(x => x.ImpactLevel).HasDefaultValue(ConceptImpact.Medium);
            e.HasIndex(x => x.Name).IsUnique();

            e.HasOne(x => x.Page)
                .WithOne()
                .HasForeignKey<Concept>(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PageId).IsUnique();
        });
    }

    private static void ConfigureCommunity(ModelBuilder b)
    {
        b.Entity<Role>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.Property(x => x.Permissions).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<User>(e =>
        {
            e.Property(x => x.Email).HasMaxLength(255).IsRequired();
            e.Property(x => x.Username).HasMaxLength(50).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.AvatarUrl).HasMaxLength(500);
            e.Property(x => x.Bio).HasColumnType("text");
            e.Property(x => x.CurrentChapter).HasDefaultValue(0);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Username).IsUnique();

            e.HasOne(x => x.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            e.ToTable(t => t.HasCheckConstraint("ck_user_current_chapter", "current_chapter >= 0"));
        });

        b.Entity<Comment>(e =>
        {
            e.Property(x => x.Body).HasColumnType("text").IsRequired();
            e.Property(x => x.IsDeleted).HasDefaultValue(false);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

            e.HasOne(x => x.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Page)
                .WithMany(p => p.Comments)
                .HasForeignKey(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(x => x.ParentCommentId)
                .OnDelete(DeleteBehavior.SetNull);

            e.ToTable(t => t.HasCheckConstraint("ck_comment_chapter_at_post", "chapter_at_post >= 0"));
        });

        b.Entity<CommentReaction>(e =>
        {
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

            e.HasOne(x => x.Comment)
                .WithMany(c => c.Reactions)
                .HasForeignKey(x => x.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.User)
                .WithMany(u => u.CommentReactions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.CommentId, x.UserId, x.ReactionType }).IsUnique();
        });

        b.Entity<EditSuggestion>(e =>
        {
            e.Property(x => x.ProposedChanges).HasColumnType("jsonb").IsRequired();
            e.Property(x => x.Reason).HasColumnType("text");
            e.Property(x => x.Status).HasDefaultValue(EditSuggestionStatus.Pending);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

            e.HasOne(x => x.User)
                .WithMany(u => u.EditSuggestions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Page)
                .WithMany(p => p.EditSuggestions)
                .HasForeignKey(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.ReviewedByUser)
                .WithMany(u => u.ReviewedEditSuggestions)
                .HasForeignKey(x => x.ReviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Bookmark>(e =>
        {
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

            e.HasOne(x => x.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Page)
                .WithMany(p => p.Bookmarks)
                .HasForeignKey(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.UserId, x.PageId }).IsUnique();
        });

        b.Entity<Notification>(e =>
        {
            e.Property(x => x.Payload).HasColumnType("jsonb");
            e.Property(x => x.IsRead).HasDefaultValue(false);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

            e.HasOne(x => x.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureHelpers(ModelBuilder b)
    {
        b.Entity<Media>(e =>
        {
            e.Property(x => x.Url).HasMaxLength(500).IsRequired();
            e.Property(x => x.AltText).HasMaxLength(255);
            e.Property(x => x.MimeType).HasMaxLength(50).IsRequired();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

            e.HasIndex(x => x.Url).IsUnique();

            e.HasOne(x => x.UploadedByUser)
                .WithMany(u => u.UploadedMedia)
                .HasForeignKey(x => x.UploadedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Tag>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(60).IsRequired();
            e.Property(x => x.Color).HasMaxLength(7);

            e.HasIndex(x => x.Name).IsUnique();
            e.HasIndex(x => x.Slug).IsUnique();

            e.ToTable(t => t.HasCheckConstraint(
                "ck_tag_color_hex",
                "color IS NULL OR color ~ '^#[0-9A-Fa-f]{6}$'"));
        });
    }

    private static void ConfigurePivots(ModelBuilder b)
    {
        b.Entity<CharacterStigma>(e =>
        {
            e.Property(x => x.IsPrimary).HasDefaultValue(false);

            e.HasOne(x => x.Character)
                .WithMany(c => c.CharacterStigmas)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Stigma)
                .WithMany(s => s.CharacterStigmas)
                .HasForeignKey(x => x.StigmaId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.CharacterId, x.StigmaId }).IsUnique();
        });

        b.Entity<CharacterAttribute>(e =>
        {
            e.HasOne(x => x.Character)
                .WithMany(c => c.CharacterAttributes)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Attribute)
                .WithMany(a => a.CharacterAttributes)
                .HasForeignKey(x => x.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.CharacterId, x.AttributeId }).IsUnique();
        });

        b.Entity<CharacterSkill>(e =>
        {
            e.Property(x => x.Level).HasDefaultValue((short)1);

            e.HasOne(x => x.Character)
                .WithMany(c => c.CharacterSkills)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Skill)
                .WithMany(s => s.CharacterSkills)
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.CharacterId, x.SkillId }).IsUnique();

            e.ToTable(t => t.HasCheckConstraint("ck_character_skill_level", "level BETWEEN 1 AND 10"));
        });

        b.Entity<CharacterItem>(e =>
        {
            e.HasOne(x => x.Character)
                .WithMany(c => c.CharacterItems)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Item)
                .WithMany(i => i.CharacterItems)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<CharacterFable>(e =>
        {
            e.HasOne(x => x.Character)
                .WithMany(c => c.CharacterFables)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Fable)
                .WithMany(f => f.CharacterFables)
                .HasForeignKey(x => x.FableId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.CharacterId, x.FableId }).IsUnique();
        });

        b.Entity<CharacterConstellation>(e =>
        {
            e.HasOne(x => x.Character)
                .WithMany(c => c.CharacterConstellations)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Constellation)
                .WithMany(c => c.CharacterConstellations)
                .HasForeignKey(x => x.ConstellationId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.CharacterId, x.ConstellationId, x.RelationshipType }).IsUnique();
        });

        b.Entity<EventCharacter>(e =>
        {
            e.HasOne(x => x.Event)
                .WithMany(ev => ev.EventCharacters)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Character)
                .WithMany(c => c.EventCharacters)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.EventId, x.CharacterId, x.Role }).IsUnique();
        });

        b.Entity<ScenarioParticipant>(e =>
        {
            e.HasOne(x => x.Scenario)
                .WithMany(s => s.ScenarioParticipants)
                .HasForeignKey(x => x.ScenarioId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Character)
                .WithMany(c => c.ScenarioParticipants)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.ScenarioId, x.CharacterId }).IsUnique();
        });

        b.Entity<ScenarioLocation>(e =>
        {
            e.HasOne(x => x.Scenario)
                .WithMany(s => s.ScenarioLocations)
                .HasForeignKey(x => x.ScenarioId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Location)
                .WithMany(l => l.ScenarioLocations)
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.ScenarioId, x.LocationId }).IsUnique();
        });

        b.Entity<PageTag>(e =>
        {
            e.HasOne(x => x.Page)
                .WithMany(p => p.PageTags)
                .HasForeignKey(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Tag)
                .WithMany(t => t.PageTags)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.PageId, x.TagId }).IsUnique();
        });

        b.Entity<Jump>(e =>
        {
            e.Property(x => x.CharacterLabel).HasMaxLength(150).IsRequired();
            e.Property(x => x.Description).HasColumnType("text");
            e.Property(x => x.LengthEstimate).HasColumnType("text");

            e.HasOne(x => x.SourceWorldline)
                .WithMany(w => w.OutgoingJumps)
                .HasForeignKey(x => x.SourceWorldlineId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.TargetWorldline)
                .WithMany(w => w.IncomingJumps)
                .HasForeignKey(x => x.TargetWorldlineId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Arc)
                .WithMany()
                .HasForeignKey(x => x.ArcId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => x.SourceWorldlineId);
            e.HasIndex(x => x.TargetWorldlineId);

            e.ToTable(t => t.HasCheckConstraint(
                "ck_jump_cross_worldline",
                "source_worldline_id <> target_worldline_id"));
        });
    }
}
