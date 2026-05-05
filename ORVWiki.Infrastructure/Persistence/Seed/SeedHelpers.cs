using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Entities;
using ORVWiki.Application.Entities.Pivots;
using ORVWiki.Application.Enums;
using AttributeEntity = ORVWiki.Application.Entities.Attribute;

namespace ORVWiki.Infrastructure.Persistence.Seed;

/// <summary>
/// Idempotent upsert helpers keyed by natural identifiers (slug, name, line_number…).
/// Re-running the seeder must not create duplicates and must not fail.
/// </summary>
internal static class SeedHelpers
{
    // ----- Page upsert (every encyclopedic entity has one) -----

    public static async Task<Page> UpsertPageAsync(
        AppDbContext db,
        string slug,
        string title,
        EntityType entityType,
        int discoveryChapter,
        string? shortDescription = null,
        CancellationToken ct = default)
    {
        var page = await db.Pages.FirstOrDefaultAsync(p => p.Slug == slug, ct);
        if (page is null)
        {
            page = new Page
            {
                Slug = slug,
                Title = title,
                EntityType = entityType,
                DiscoveryChapter = discoveryChapter,
                ShortDescription = shortDescription,
            };
            db.Pages.Add(page);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            page.Title = title;
            page.EntityType = entityType;
            page.DiscoveryChapter = discoveryChapter;
            page.ShortDescription = shortDescription;
        }
        return page;
    }

    // ----- Domain entity upserts (paired 1:1 with a Page) -----

    public static async Task<Arc> UpsertArcAsync(
        AppDbContext db, string slug, string name, short orderNumber,
        int chapterStart, int chapterEnd, string summary, CancellationToken ct)
    {
        var page = await UpsertPageAsync(db, slug, name, EntityType.Arc, chapterStart,
            shortDescription: $"Arc {orderNumber}: chapters {chapterStart}–{chapterEnd}", ct: ct);
        var arc = await db.Arcs.FirstOrDefaultAsync(a => a.PageId == page.Id, ct);
        if (arc is null)
        {
            arc = new Arc
            {
                PageId = page.Id, Name = name, OrderNumber = orderNumber,
                ChapterStart = chapterStart, ChapterEnd = chapterEnd, Summary = summary,
            };
            db.Arcs.Add(arc);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            arc.Name = name; arc.OrderNumber = orderNumber;
            arc.ChapterStart = chapterStart; arc.ChapterEnd = chapterEnd; arc.Summary = summary;
        }
        return arc;
    }

    public static async Task<Chapter> UpsertChapterAsync(
        AppDbContext db, int chapterNumber, string title, long arcId, string? summary,
        CancellationToken ct)
    {
        var ch = await db.Chapters.FindAsync([chapterNumber], ct);
        if (ch is null)
        {
            ch = new Chapter
            {
                ChapterNumber = chapterNumber, Title = title, ArcId = arcId, Summary = summary,
            };
            db.Chapters.Add(ch);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            ch.Title = title; ch.ArcId = arcId; ch.Summary = summary;
        }
        return ch;
    }

    public static async Task<Character> UpsertCharacterAsync(
        AppDbContext db, string slug, string fullName, int discoveryChapter,
        Action<Character> configure, string? shortDescription = null, CancellationToken ct = default)
    {
        var page = await UpsertPageAsync(db, slug, fullName, EntityType.Character,
            discoveryChapter, shortDescription, ct);
        var c = await db.Characters.FirstOrDefaultAsync(x => x.PageId == page.Id, ct);
        if (c is null)
        {
            c = new Character { PageId = page.Id, FullName = fullName };
            configure(c);
            db.Characters.Add(c);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            c.FullName = fullName;
            configure(c);
        }
        return c;
    }

    public static async Task<Constellation> UpsertConstellationAsync(
        AppDbContext db, string slug, string modifier, ConstellationGrade grade,
        int discoveryChapter, string? trueName = null, string? description = null,
        long? originCharacterId = null, long? nebulaId = null, CancellationToken ct = default)
    {
        var page = await UpsertPageAsync(db, slug, modifier, EntityType.Constellation,
            discoveryChapter, description, ct);
        var c = await db.Constellations.FirstOrDefaultAsync(x => x.PageId == page.Id, ct);
        if (c is null)
        {
            c = new Constellation
            {
                PageId = page.Id, Modifier = modifier, Grade = grade,
                TrueName = trueName, Description = description,
                OriginCharacterId = originCharacterId, NebulaId = nebulaId,
            };
            db.Constellations.Add(c);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            c.Modifier = modifier; c.Grade = grade; c.TrueName = trueName;
            c.Description = description; c.OriginCharacterId = originCharacterId;
            c.NebulaId = nebulaId;
        }
        return c;
    }

    public static async Task<Dokkaebi> UpsertDokkaebiAsync(
        AppDbContext db, string slug, string name, DokkaebiRank rank, int discoveryChapter,
        string? channelId = null, string? speciality = null, CancellationToken ct = default)
    {
        var page = await UpsertPageAsync(db, slug, name, EntityType.Dokkaebi,
            discoveryChapter, speciality, ct);
        var d = await db.Dokkaebi.FirstOrDefaultAsync(x => x.PageId == page.Id, ct);
        if (d is null)
        {
            d = new Dokkaebi
            {
                PageId = page.Id, Name = name, Rank = rank,
                ChannelId = channelId, Speciality = speciality,
            };
            db.Dokkaebi.Add(d);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            d.Name = name; d.Rank = rank; d.ChannelId = channelId; d.Speciality = speciality;
        }
        return d;
    }

    public static async Task<Stigma> UpsertStigmaAsync(
        AppDbContext db, string slug, string name, long providerConstellationId,
        string effect, int discoveryChapter, int activationCost = 0,
        CancellationToken ct = default)
    {
        var page = await UpsertPageAsync(db, slug, name, EntityType.Stigma, discoveryChapter, effect, ct);
        var s = await db.Stigmas.FirstOrDefaultAsync(x => x.PageId == page.Id, ct);
        if (s is null)
        {
            s = new Stigma
            {
                PageId = page.Id, Name = name, Effect = effect,
                ProviderConstellationId = providerConstellationId, ActivationCost = activationCost,
            };
            db.Stigmas.Add(s);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            s.Name = name; s.Effect = effect;
            s.ProviderConstellationId = providerConstellationId;
            s.ActivationCost = activationCost;
        }
        return s;
    }

    public static async Task<AttributeEntity> UpsertAttributeAsync(
        AppDbContext db, string slug, string name, AttributeRarity rarity,
        string? effect, int discoveryChapter, CancellationToken ct = default)
    {
        var page = await UpsertPageAsync(db, slug, name, EntityType.Attribute, discoveryChapter, effect, ct);
        var a = await db.Attributes.FirstOrDefaultAsync(x => x.PageId == page.Id, ct);
        if (a is null)
        {
            a = new AttributeEntity { PageId = page.Id, Name = name, Rarity = rarity, Effect = effect };
            db.Attributes.Add(a);
            await db.SaveChangesAsync(ct);
        }
        else { a.Name = name; a.Rarity = rarity; a.Effect = effect; }
        return a;
    }

    public static async Task<Skill> UpsertSkillAsync(
        AppDbContext db, string slug, string name, SkillType type,
        string? effect, int discoveryChapter, int? costInCoins = null, short? maxLevel = 10,
        CancellationToken ct = default)
    {
        var page = await UpsertPageAsync(db, slug, name, EntityType.Skill, discoveryChapter, effect, ct);
        var s = await db.Skills.FirstOrDefaultAsync(x => x.PageId == page.Id, ct);
        if (s is null)
        {
            s = new Skill
            {
                PageId = page.Id, Name = name, SkillType = type, Effect = effect,
                CostInCoins = costInCoins, MaxLevel = maxLevel,
            };
            db.Skills.Add(s);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            s.Name = name; s.SkillType = type; s.Effect = effect;
            s.CostInCoins = costInCoins; s.MaxLevel = maxLevel;
        }
        return s;
    }

    public static async Task<Item> UpsertItemAsync(
        AppDbContext db, string slug, string name, ItemGrade grade,
        string? description, int discoveryChapter, bool isStarRelic = false,
        CancellationToken ct = default)
    {
        var page = await UpsertPageAsync(db, slug, name, EntityType.Item, discoveryChapter, description, ct);
        var i = await db.Items.FirstOrDefaultAsync(x => x.PageId == page.Id, ct);
        if (i is null)
        {
            i = new Item
            {
                PageId = page.Id, Name = name, ItemGrade = grade,
                Description = description, IsStarRelic = isStarRelic,
            };
            db.Items.Add(i);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            i.Name = name; i.ItemGrade = grade; i.Description = description; i.IsStarRelic = isStarRelic;
        }
        return i;
    }

    public static async Task<Location> UpsertLocationAsync(
        AppDbContext db, string slug, string name, int discoveryChapter,
        string? dimension = null, long? worldlineId = null, long? parentLocationId = null,
        string? description = null, CancellationToken ct = default)
    {
        var page = await UpsertPageAsync(db, slug, name, EntityType.Location, discoveryChapter, description, ct);
        var l = await db.Locations.FirstOrDefaultAsync(x => x.PageId == page.Id, ct);
        if (l is null)
        {
            l = new Location
            {
                PageId = page.Id, Name = name, Dimension = dimension,
                WorldlineId = worldlineId, ParentLocationId = parentLocationId,
                Description = description,
            };
            db.Locations.Add(l);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            l.Name = name; l.Dimension = dimension;
            l.WorldlineId = worldlineId; l.ParentLocationId = parentLocationId;
            l.Description = description;
        }
        return l;
    }

    public static async Task<Worldline> UpsertWorldlineAsync(
        AppDbContext db, string slug, int lineNumber, string? name, bool isMain,
        int discoveryChapter, string? description = null, long? parentWorldlineId = null,
        CancellationToken ct = default)
    {
        var page = await UpsertPageAsync(db, slug, name ?? $"{lineNumber}th Worldline",
            EntityType.Worldline, discoveryChapter, description, ct);
        var w = await db.Worldlines.FirstOrDefaultAsync(x => x.PageId == page.Id, ct);
        if (w is null)
        {
            w = new Worldline
            {
                PageId = page.Id, LineNumber = lineNumber, Name = name, IsMain = isMain,
                Description = description, ParentWorldlineId = parentWorldlineId,
            };
            db.Worldlines.Add(w);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            w.LineNumber = lineNumber; w.Name = name; w.IsMain = isMain;
            w.Description = description; w.ParentWorldlineId = parentWorldlineId;
        }
        return w;
    }

    public static async Task<Scenario> UpsertScenarioAsync(
        AppDbContext db, string slug, string code, string title, ScenarioType type,
        ScenarioDifficulty difficulty, int chapterStart, int? chapterEnd,
        string? conditions, string? rewards, string? penalty, CancellationToken ct = default)
    {
        var page = await UpsertPageAsync(db, slug, title, EntityType.Scenario, chapterStart,
            shortDescription: $"{type} scenario [{code}] (difficulty {difficulty})", ct: ct);
        var s = await db.Scenarios.FirstOrDefaultAsync(x => x.PageId == page.Id, ct);
        if (s is null)
        {
            s = new Scenario
            {
                PageId = page.Id, Code = code, Title = title, Type = type, Difficulty = difficulty,
                ChapterStart = chapterStart, ChapterEnd = chapterEnd,
                Conditions = conditions, Rewards = rewards, Penalty = penalty,
            };
            db.Scenarios.Add(s);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            s.Code = code; s.Title = title; s.Type = type; s.Difficulty = difficulty;
            s.ChapterStart = chapterStart; s.ChapterEnd = chapterEnd;
            s.Conditions = conditions; s.Rewards = rewards; s.Penalty = penalty;
        }
        return s;
    }

    public static async Task<Event> UpsertEventAsync(
        AppDbContext db, string slug, string title, int chapterNumber,
        EventImportance importance, string? description, int? eventOrder = null,
        long? locationId = null, long? worldlineId = null, CancellationToken ct = default)
    {
        var page = await UpsertPageAsync(db, slug, title, EntityType.Event, chapterNumber, description, ct);
        var e = await db.Events.FirstOrDefaultAsync(x => x.PageId == page.Id, ct);
        if (e is null)
        {
            e = new Event
            {
                PageId = page.Id, Title = title, Description = description,
                ChapterNumber = chapterNumber, Importance = importance,
                EventOrder = eventOrder, LocationId = locationId, WorldlineId = worldlineId,
            };
            db.Events.Add(e);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            e.Title = title; e.Description = description;
            e.ChapterNumber = chapterNumber; e.Importance = importance;
            e.EventOrder = eventOrder; e.LocationId = locationId; e.WorldlineId = worldlineId;
        }
        return e;
    }

    public static async Task<Concept> UpsertConceptAsync(
        AppDbContext db, string slug, string name, string definition, ConceptImpact impact,
        int discoveryChapter, CancellationToken ct = default)
    {
        var page = await UpsertPageAsync(db, slug, name, EntityType.Concept, discoveryChapter, definition, ct);
        var c = await db.Concepts.FirstOrDefaultAsync(x => x.PageId == page.Id, ct);
        if (c is null)
        {
            c = new Concept { PageId = page.Id, Name = name, Definition = definition, ImpactLevel = impact };
            db.Concepts.Add(c);
            await db.SaveChangesAsync(ct);
        }
        else { c.Name = name; c.Definition = definition; c.ImpactLevel = impact; }
        return c;
    }

    public static async Task<Tag> UpsertTagAsync(
        AppDbContext db, string slug, string name, string? color, CancellationToken ct = default)
    {
        var t = await db.Tags.FirstOrDefaultAsync(x => x.Slug == slug, ct);
        if (t is null)
        {
            t = new Tag { Slug = slug, Name = name, Color = color };
            db.Tags.Add(t);
            await db.SaveChangesAsync(ct);
        }
        else { t.Name = name; t.Color = color; }
        return t;
    }

    // ----- Pivot upserts -----

    public static async Task LinkCharacterAttributeAsync(
        AppDbContext db, long characterId, long attributeId, int? acquiredChapter, CancellationToken ct)
    {
        if (await db.CharacterAttributes.AnyAsync(
                x => x.CharacterId == characterId && x.AttributeId == attributeId, ct)) return;
        db.CharacterAttributes.Add(new CharacterAttribute
        {
            CharacterId = characterId, AttributeId = attributeId, AcquiredChapter = acquiredChapter,
        });
        await db.SaveChangesAsync(ct);
    }

    public static async Task LinkCharacterSkillAsync(
        AppDbContext db, long characterId, long skillId, short level, int? acquiredChapter,
        CancellationToken ct)
    {
        if (await db.CharacterSkills.AnyAsync(
                x => x.CharacterId == characterId && x.SkillId == skillId, ct)) return;
        db.CharacterSkills.Add(new CharacterSkill
        {
            CharacterId = characterId, SkillId = skillId, Level = level, AcquiredChapter = acquiredChapter,
        });
        await db.SaveChangesAsync(ct);
    }

    public static async Task LinkCharacterStigmaAsync(
        AppDbContext db, long characterId, long stigmaId, bool isPrimary, int? acquiredChapter,
        CancellationToken ct)
    {
        if (await db.CharacterStigmas.AnyAsync(
                x => x.CharacterId == characterId && x.StigmaId == stigmaId, ct)) return;
        db.CharacterStigmas.Add(new CharacterStigma
        {
            CharacterId = characterId, StigmaId = stigmaId,
            IsPrimary = isPrimary, AcquiredChapter = acquiredChapter,
        });
        await db.SaveChangesAsync(ct);
    }

    public static async Task LinkCharacterItemAsync(
        AppDbContext db, long characterId, long itemId, int? acquiredChapter, int? lostChapter,
        CancellationToken ct)
    {
        // CharacterItem has no composite unique — item can be re-acquired. Use a coarse dedupe:
        // skip if there's already a row with the same acquired_chapter (treat as same acquisition).
        if (await db.CharacterItems.AnyAsync(
                x => x.CharacterId == characterId && x.ItemId == itemId
                  && x.AcquiredChapter == acquiredChapter, ct)) return;
        db.CharacterItems.Add(new CharacterItem
        {
            CharacterId = characterId, ItemId = itemId,
            AcquiredChapter = acquiredChapter, LostChapter = lostChapter,
        });
        await db.SaveChangesAsync(ct);
    }

    public static async Task LinkCharacterConstellationAsync(
        AppDbContext db, long characterId, long constellationId,
        CharacterConstellationRel rel, int? sinceChapter, CancellationToken ct)
    {
        if (await db.CharacterConstellations.AnyAsync(
                x => x.CharacterId == characterId && x.ConstellationId == constellationId
                  && x.RelationshipType == rel, ct)) return;
        db.CharacterConstellations.Add(new CharacterConstellation
        {
            CharacterId = characterId, ConstellationId = constellationId,
            RelationshipType = rel, SinceChapter = sinceChapter,
        });
        await db.SaveChangesAsync(ct);
    }

    public static async Task LinkEventCharacterAsync(
        AppDbContext db, long eventId, long characterId, EventCharacterRole role, CancellationToken ct)
    {
        if (await db.EventCharacters.AnyAsync(
                x => x.EventId == eventId && x.CharacterId == characterId && x.Role == role, ct)) return;
        db.EventCharacters.Add(new EventCharacter
        {
            EventId = eventId, CharacterId = characterId, Role = role,
        });
        await db.SaveChangesAsync(ct);
    }

    public static async Task LinkScenarioParticipantAsync(
        AppDbContext db, long scenarioId, long characterId, ScenarioOutcome outcome, CancellationToken ct)
    {
        if (await db.ScenarioParticipants.AnyAsync(
                x => x.ScenarioId == scenarioId && x.CharacterId == characterId, ct)) return;
        db.ScenarioParticipants.Add(new ScenarioParticipant
        {
            ScenarioId = scenarioId, CharacterId = characterId, Outcome = outcome,
        });
        await db.SaveChangesAsync(ct);
    }

    public static async Task LinkScenarioLocationAsync(
        AppDbContext db, long scenarioId, long locationId, CancellationToken ct)
    {
        if (await db.ScenarioLocations.AnyAsync(
                x => x.ScenarioId == scenarioId && x.LocationId == locationId, ct)) return;
        db.ScenarioLocations.Add(new ScenarioLocation
        {
            ScenarioId = scenarioId, LocationId = locationId,
        });
        await db.SaveChangesAsync(ct);
    }

    public static async Task LinkPageTagAsync(
        AppDbContext db, long pageId, short tagId, CancellationToken ct)
    {
        if (await db.PageTags.AnyAsync(x => x.PageId == pageId && x.TagId == tagId, ct)) return;
        db.PageTags.Add(new PageTag { PageId = pageId, TagId = tagId });
        await db.SaveChangesAsync(ct);
    }
}
