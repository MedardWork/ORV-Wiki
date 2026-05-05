using Microsoft.EntityFrameworkCore;
using ORVWiki.Application.Enums;
using static ORVWiki.Infrastructure.Persistence.Seed.SeedHelpers;

namespace ORVWiki.Infrastructure.Persistence.Seed;

/// <summary>
/// Arc 1 — "Three Ways to Survive in a Ruined World" (chapters 1–23).
/// Episodes 1–5: Starting the Paid Service · Protagonist · Contract · Line of Hypocrisy · Dark Keeper.
///
/// Spoiler conventions used here (read both before editing):
///
/// 1. The page-level gate (Page.discovery_chapter) decides whether a page surfaces at all.
///    A page introduced at chapter N is invisible to readers at current_chapter &lt; N. The
///    discoveryChapter we pick must be the first chapter at which a *spoiler-conscious*
///    reader could be told the page exists without spoiling anything they have not read.
///
/// 2. Inline `[spoiler ch=N]…[/spoiler]` markup blacks out a passage until the reader's
///    current_chapter &gt;= N. Hidden segments never reach the client (server-enforced).
///    Use these for facts that the *page itself* implies but that the reader at
///    discoveryChapter cannot yet know.
///
/// 3. Identity reveals are split across separate Character rows. We never write
///    `Person A is actually Person B` on a page that is visible before the reveal —
///    we make A and B two separate pages, each carrying a `[spoiler ch=N]` block at the
///    canonical reveal chapter. This is why tls123 (visible from ch 1) and Han Sooyoung
///    (visible from ch 68) are separate Character rows, with the connection gated to
///    the chapter where the canonical text states `Han Sooyoung was tls123`.
///
/// 4. Structural fields on entities (Status, DeathChapter, Dimension, TrueName, …) are
///    plain strings — they are NOT routed through RenderInline. So we keep them at the
///    spoiler-safe level for the page's own discoveryChapter. Death and identity facts
///    that would pre-spoil go into the narrative field (Biography / Description) wrapped
///    in `[spoiler ch=N]`, while the structural columns stay neutral.
/// </summary>
internal static class Arc01_ThreeWaysToSurvive
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct)
    {
        // -------------------------------------------------------------------
        // Worldlines (the 3D timeline backbone — created once for the project)
        // -------------------------------------------------------------------
        // The fact that this is the 1864th iteration is a deep mid-novel
        // reveal. The number "1864" is first uttered in the canonical text
        // around ch 297 (Ep. 56 — Yoo Joonghyuk in the white coat). Until then
        // the reader knows there have been "many regressions" but not the count.
        // The pages stay hidden until ch 297. The rows still exist (events
        // reference them by FK) — they just don't surface in the wiki listing
        // for early-chapter readers.
        var mainLine = await UpsertWorldlineAsync(db,
            slug: "1864th-worldline", lineNumber: 1864,
            name: "1864th Worldline", isMain: true, discoveryChapter: 297,
            description:
                "[spoiler ch=297]The current worldline followed by Kim Dokja and the survivors of " +
                "carriage 3807. The 1864th iteration of the *Three Ways to Survive in a Ruined World* " +
                "timeline — the result of more than a thousand prior regressions, each ending in the " +
                "death of the protagonist and the rewinding of the scenarios to their start.[/spoiler]",
            ct: ct);

        var prevLine = await UpsertWorldlineAsync(db,
            slug: "1863rd-worldline", lineNumber: 1863,
            name: "1863rd Worldline", isMain: false, discoveryChapter: 266,
            description:
                "[spoiler ch=266]The immediately preceding regression. The worldline followed by the " +
                "canonical text of *Ways of Survival* — i.e. the version Kim Dokja read for ten years. " +
                "Yoo Junghyuk's memories at the start of the 1864th round are inherited from this line.[/spoiler]",
            ct: ct);
        // Regression chain: 1864 ← 1863
        var mainLineEntity = db.Worldlines.Find(mainLine.Id);
        if (mainLineEntity is not null && mainLineEntity.ParentWorldlineId != prevLine.Id)
        {
            mainLineEntity.ParentWorldlineId = prevLine.Id;
            await db.SaveChangesAsync(ct);
        }

        // -------------------------------------------------------------------
        // Tags
        // -------------------------------------------------------------------
        var tagProtagonist = await UpsertTagAsync(db, "protagonist", "Protagonist", "#FFD700", ct);
        var tagIncarnation = await UpsertTagAsync(db, "incarnation", "Incarnation", "#4A90E2", ct);
        var tagConstellation = await UpsertTagAsync(db, "constellation", "Constellation", "#9B59B6", ct);
        var tagDokkaebi = await UpsertTagAsync(db, "dokkaebi", "Dokkaebi", "#E67E22", ct);
        var tagArc1 = await UpsertTagAsync(db, "arc-1", "Arc 1", "#27AE60", ct);
        var tagSeoul = await UpsertTagAsync(db, "seoul", "Seoul", "#34495E", ct);

        // -------------------------------------------------------------------
        // Arc + Chapters (the spine)
        // -------------------------------------------------------------------
        var arc = await UpsertArcAsync(db,
            slug: "arc-three-ways-to-survive",
            name: "Three Ways to Survive in a Ruined World",
            orderNumber: 1, chapterStart: 1, chapterEnd: 23,
            summary:
                "The opening arc. Office worker Kim Dokja boards subway line 3 home from work, only to " +
                "find the world plunged into the scenarios of the very web novel he has read for over " +
                "a decade. As the dokkaebi Bihyung opens the channel above carriage 3807, Dokja realises " +
                "he is the only person on Earth who has read what comes next.\n\n" +
                "[spoiler ch=6]The first divergence comes early: rather than let the carriage descend " +
                "into mob violence, Dokja kills Kim Namwoon himself to break the murderer scenario.[/spoiler]\n\n" +
                "[spoiler ch=11]On Dongho Bridge the survivors meet Yoo Junghyuk — the man Dokja has " +
                "been reading about for ten years — and Dokja takes the impossible gamble of jumping " +
                "into the mouth of an ichthyosaur to open a hidden scenario.[/spoiler]\n\n" +
                "[spoiler ch=23]The arc closes deep in Dark Root with the slaying of the Dark Keeper, " +
                "the recovery of Yoo Sangah, and the death of Han Myungoh.[/spoiler]",
            ct: ct);

        // Chapter list — each title comes from the epub TOC.
        // Summaries kept terse; deeper plot content lives on Event entities.
        var ch1  = await UpsertChapterAsync(db,  1, "Prologue – Three Ways to Survive in a Ruined World", arc.Id,
            "Kim Dokja finishes the 3,149-chapter web novel *Ways of Survival* by author tls123. The author messages him a thank-you and sends a gift file by email.", ct);
        var ch2  = await UpsertChapterAsync(db,  2, "Ep. 1 – Starting the Paid Service, I", arc.Id,
            "On the subway home with HR colleague Yoo Sangah, the train stops between stations and the lights cut out — the opening scene of the novel he just finished reading.", ct);
        var ch3  = await UpsertChapterAsync(db,  3, "Ep. 1 – Starting the Paid Service, II", arc.Id,
            "Bihyung the dokkaebi appears in carriage 3807 and silences the protesting passengers by bursting their heads. The first scenario is announced.", ct);
        var ch4  = await UpsertChapterAsync(db,  4, "Ep. 1 – Starting the Paid Service, III", arc.Id,
            "Disclaimer scenario: each passenger must kill a cockroach within five minutes or die. Lee Gilyoung struggles; Kim Dokja helps him.", ct);
        var ch5  = await UpsertChapterAsync(db,  5, "Ep. 1 – Starting the Paid Service, IV", arc.Id,
            "Dokja runs the gift file from tls123, gaining an exclusive attribute and skill slot. He cannot open his own attribute window. Kim Namwoon begins inciting the passengers to murder.", ct);
        var ch6  = await UpsertChapterAsync(db,  6, "Ep. 1 – Starting the Paid Service, V", arc.Id,
            "Murderer scenario announced: kill one passenger within thirty minutes. Dokja kills Kim Namwoon to break the script and save the rest of the carriage.", ct);
        var ch7  = await UpsertChapterAsync(db,  7, "Ep. 2 – Protagonist, I", arc.Id,
            "Survivors of carriage 3807: Kim Dokja, Lee Hyunsung, Yoo Sangah, Han Myungoh and Lee Gilyoung. Twenty-one constellations are now connected to Bihyung's channel.", ct);
        var ch8  = await UpsertChapterAsync(db,  8, "Ep. 2 – Protagonist, II", arc.Id,
            "Crossing Dongho Bridge scenario. Kim Namwoon resurrects as a 9th-grade demonic person and pursues Dokja. Han Myungoh receives a stigma from a constellation and abducts Yoo Sangah.", ct);
        var ch9  = await UpsertChapterAsync(db,  9, "Ep. 2 – Protagonist, III", arc.Id,
            "Dokja meets Yoo Junghyuk, protagonist of *Ways of Survival*, who emerges from carriage 3707 to deal with the demonic horde.", ct);
        var ch10 = await UpsertChapterAsync(db, 10, "Ep. 2 – Protagonist, IV", arc.Id,
            "Outnumbered on the bridge, Dokja accumulates coins and prepares to risk a hidden scenario by leaping into the Han River below.", ct);
        var ch11 = await UpsertChapterAsync(db, 11, "Ep. 2 – Protagonist, V", arc.Id,
            "Dokja jumps and is swallowed whole by an ichthyosaur — the entry-condition for the hidden scenario *Slay the Ichthyosaur from Within*.", ct);
        var ch12 = await UpsertChapterAsync(db, 12, "Ep. 3 – Contract, I", arc.Id,
            "Inside the ichthyosaur Dokja drinks hammer-seahorse mucus to coat his skin against the digestive juices and stabs the stomach wall with a stone hog's thorn.", ct);
        var ch13 = await UpsertChapterAsync(db, 13, "Ep. 3 – Contract, II", arc.Id,
            "Bihyung visits Dokja inside the beast and they negotiate the terms of a private contract. Dokja receives his first sponsorships from constellations.", ct);
        var ch14 = await UpsertChapterAsync(db, 14, "Ep. 3 – Contract, III", arc.Id,
            "The Demon-like Judge of Fire takes an interest in Dokja. Constellation messages flow in steadily as he endures the digestive heat.", ct);
        var ch15 = await UpsertChapterAsync(db, 15, "Ep. 3 – Contract, IV", arc.Id,
            "After four days the ichthyosaur dies. Dokja claims the hidden-scenario reward of 9,000 coins plus 1,000 first-clear bonus, and his Physique stat rises.", ct);
        var ch16 = await UpsertChapterAsync(db, 16, "Ep. 4 – Line of Hypocrisy, I", arc.Id,
            "Dokja crawls out of the carcass downriver and reaches the safety zone. Reunion with Lee Gilyoung and Lee Hyunsung; Yoo Sangah is missing.", ct);
        var ch17 = await UpsertChapterAsync(db, 17, "Ep. 4 – Line of Hypocrisy, II", arc.Id,
            "The party prepares to descend into the underground complex Dark Root in pursuit of Han Myungoh and Yoo Sangah.", ct);
        var ch18 = await UpsertChapterAsync(db, 18, "Ep. 4 – Line of Hypocrisy, III", arc.Id,
            "First contact with the ground rats and their black-ether burrows beneath Seoul.", ct);
        var ch19 = await UpsertChapterAsync(db, 19, "Ep. 4 – Line of Hypocrisy, IV", arc.Id,
            "Lee Gilyoung's exclusive skill *Diverse Communication* is tested as he scouts ahead through the cockroach swarm.", ct);
        var ch20 = await UpsertChapterAsync(db, 20, "Ep. 5 – Dark Keeper, I", arc.Id,
            "The party threads the deepest part of Dark Root, drawn by the black flames at its core.", ct);
        var ch21 = await UpsertChapterAsync(db, 21, "Ep. 5 – Dark Keeper, II", arc.Id,
            "Yoo Sangah and Han Myungoh are found bound to the floor by black-ether branches. Han Myungoh tries to blame Yoo Sangah for the apocalypse.", ct);
        var ch22 = await UpsertChapterAsync(db, 22, "Ep. 5 – Dark Keeper, III", arc.Id,
            "Dokja exposes Han Myungoh as the bicycle thief, throws his last thorn into the dark, and the boss-class Dark Keeper appears. The sub-scenario *Kill the Guard* begins.", ct);
        var ch23 = await UpsertChapterAsync(db, 23, "Ep. 5 – Dark Keeper, IV", arc.Id,
            "The Dark Keeper falls, closing the first episode of the new world. Bihyung reveals the path to the next scenario region.", ct);

        // -------------------------------------------------------------------
        // Concepts (cross-cutting in-world definitions)
        // -------------------------------------------------------------------
        await UpsertConceptAsync(db, "scenario", "Scenario",
            "A challenge issued by the Star Stream and administered by a dokkaebi channel. " +
            "Failing the conditions kills the participant; clearing them grants coins and access to " +
            "subsequent scenarios. Scenarios take precedence over outside reality — while one is in " +
            "progress no monster outside the script can interfere.",
            ConceptImpact.Core, discoveryChapter: 3, ct: ct);

        await UpsertConceptAsync(db, "coin", "Coin",
            "The Star Stream's universal currency. Earned through scenario completion and constellation " +
            "sponsorship; spent on skills, items, attribute boosts and stat increases.",
            ConceptImpact.Core, discoveryChapter: 7, ct: ct);

        await UpsertConceptAsync(db, "incarnation", "Incarnation",
            "A mortal participant in the Star Stream's scenarios. Incarnations may receive stigmas and " +
            "sponsorship from constellations who like their performance.",
            ConceptImpact.Core, discoveryChapter: 3, ct: ct);

        await UpsertConceptAsync(db, "constellation", "Constellation",
            "Beings dwelling in the distant nebulae who watch incarnations through dokkaebi channels, " +
            "sponsor coins, and bestow stigmas on those they favour.",
            ConceptImpact.Core, discoveryChapter: 7, ct: ct);

        await UpsertConceptAsync(db, "channel", "Dokkaebi Channel",
            "A broadcast that connects an incarnation's scenarios to the constellations watching from " +
            "the nebulae. Each dokkaebi maintains their own channel; more constellations means more " +
            "potential sponsorships and more rigorous scenarios.",
            ConceptImpact.High, discoveryChapter: 7, ct: ct);

        await UpsertConceptAsync(db, "stigma", "Stigma",
            "A power lent by a constellation to an incarnation through their dokkaebi's channel. " +
            "Stigmas usually carry the constellation's domain and can be activated for a coin cost.",
            ConceptImpact.High, discoveryChapter: 10, ct: ct);

        await UpsertConceptAsync(db, "demonic-person", "Demonic Person",
            "A 9th-grade species classification: a human mutated by black ether, retaining no consciousness " +
            "but possessing strength several times that of the original host. Considered high-risk despite " +
            "the low grade.",
            ConceptImpact.Medium, discoveryChapter: 8, ct: ct);

        await UpsertConceptAsync(db, "monster-grade", "Monster Grade",
            "A 1–9 classification system used in the new world. Higher grades indicate weaker species; " +
            "1st grade beings are world-class threats, 9th grade are barely above ferals. The grade does " +
            "not always reflect the danger of the host (see *Demonic Person*).",
            ConceptImpact.Medium, discoveryChapter: 7, ct: ct);

        await UpsertConceptAsync(db, "regression", "Regression",
            "A scenario phenomenon in which a being who dies returns to the start of the scenarios with " +
            "their memories intact. Regressors carry the experience of every prior life with them; their " +
            "composure under pressure is the result of having lived this script before.\n\n" +
            "[spoiler ch=266]Yoo Junghyuk's regression is administered by the constellation 'Secretive " +
            "Plotter' and tracked by the Star Stream as numbered turns. Each round wipes the world and " +
            "rewinds the scenarios; only the regressor remembers.[/spoiler]",
            ConceptImpact.Core, discoveryChapter: 11, ct: ct);

        // -------------------------------------------------------------------
        // Dokkaebi
        // -------------------------------------------------------------------
        var bihyung = await UpsertDokkaebiAsync(db,
            slug: "bihyung", name: "Bihyung",
            rank: DokkaebiRank.Low, discoveryChapter: 3,
            channelId: "#BI-7623",
            speciality:
                "The dokkaebi assigned to channel #BI-7623, broadcasting the disasters of carriage 3807. " +
                "Bihyung enters the world late — only at the moment monetisation begins — and is " +
                "described as a beginner administrator. His patter is showy and merchant-like, but his " +
                "rulings are surprisingly even-handed even when they cost his own channel reach. " +
                "[spoiler ch=15]Over the course of Episode 3 he and Kim Dokja settle into a wary working " +
                "rapport — Bihyung is the first dokkaebi to recognise that this particular incarnation " +
                "is operating on information no one else has.[/spoiler]",
            ct: ct);

        // -------------------------------------------------------------------
        // Constellations introduced in Arc 1
        // -------------------------------------------------------------------
        // All four are listed by modifier in chapter 7 as the Sponsor Selection
        // options — that is when the reader learns these names exist in this
        // story. The "true name" deduction (Sun Wukong, etc.) happens inside
        // Kim Dokja's monologue in the same chapter, so trueName + ch 7 is OK.
        var golHeadband = await UpsertConstellationAsync(db,
            slug: "prisoner-of-the-golden-headband",
            modifier: "Prisoner of the Golden Headband",
            grade: ConstellationGrade.Myth, discoveryChapter: 7,
            trueName: "Sun Wukong",
            description:
                "A great myth-grade constellation of the Journey to the West cycle. Punished for his " +
                "rebellion in heaven, the Monkey King wears the golden circlet that gives him his epithet. " +
                "Listed as one of Kim Dokja's four sponsor options in the Sponsor Selection above " +
                "carriage 3807. [spoiler ch=22]Becomes one of Kim Dokja's earliest and most loyal " +
                "sponsors at the close of the Dark Keeper encounter.[/spoiler]",
            ct: ct);

        var demonJudge = await UpsertConstellationAsync(db,
            slug: "demon-like-judge-of-fire",
            modifier: "Demon-like Judge of Fire",
            grade: ConstellationGrade.Myth, discoveryChapter: 7,
            description:
                "A constellation of judgement and flame, listed among Kim Dokja's four sponsor options " +
                "in the Sponsor Selection. [spoiler ch=10]Takes a serious interest in him on Dongho " +
                "Bridge, repeatedly admiring his spirit and willingness to suffer, and sponsors coins " +
                "throughout the early scenarios.[/spoiler]",
            ct: ct);

        var secretivePlotter = await UpsertConstellationAsync(db,
            slug: "secretive-plotter",
            modifier: "Secretive Plotter",
            grade: ConstellationGrade.Historical, discoveryChapter: 7,
            description:
                "A constellation that delights in deception and outmanoeuvre. Kim Dokja remarks at the " +
                "Sponsor Selection that this is the first time he has seen this option in *Ways of " +
                "Survival* — so plain a modifier that he assumes the constellation behind it is weak. " +
                "[spoiler ch=10]Increasingly amused by, and impressed with, Kim Dokja as the carriage-" +
                "3807 storyline diverges from the script.[/spoiler]\n\n" +
                "[spoiler ch=266]Revealed much later as the constellation responsible for Yoo Junghyuk's " +
                "regression — the figure who has watched, and partly directed, every one of his lives.[/spoiler]",
            ct: ct);

        var lameTrickster = await UpsertConstellationAsync(db,
            slug: "lame-trickster",
            modifier: "Lame Trickster",
            grade: ConstellationGrade.Historical, discoveryChapter: 10,
            description:
                "An obscure constellation of cunning who normally does not bestow stigmas through the " +
                "standard mental-barrier system. Its sponsorship of Han Myungoh on Dongho Bridge is " +
                "Kim Dokja's first hint that off-channel deals are possible.",
            ct: ct);

        var blackFlameDragon = await UpsertConstellationAsync(db,
            slug: "abyssal-black-flame-dragon",
            modifier: "Abyssal Black Flame Dragon",
            grade: ConstellationGrade.Myth, discoveryChapter: 7,
            description:
                "An abyssal myth-grade constellation, listed among Kim Dokja's four Sponsor Selection " +
                "options. Leader of the constellation faction known as the Black Cloud; favours " +
                "incarnations with the *chuuni* attribute and grants powerful early-game attack output " +
                "at the cost of mental corruption. [spoiler ch=22]Openly hostile to Kim Dokja from the " +
                "Dark Keeper encounter onward, expressing disappointment each time Dokja survives a " +
                "scenario the constellation expected to kill him.[/spoiler]",
            ct: ct);

        // -------------------------------------------------------------------
        // Stigmas
        // -------------------------------------------------------------------
        await UpsertStigmaAsync(db,
            slug: "stigma-one-legged-swift-horse",
            name: "One-legged Swift Horse",
            providerConstellationId: lameTrickster.Id,
            effect:
                "Grants the bearer the running speed of an Olympic athlete on a single leg. " +
                "Lent to Han Myungoh on Dongho Bridge so he could flee Kim Dokja with Yoo Sangah in tow. " +
                "An off-system stigma not normally distributed through the standard mental-barrier channel.",
            discoveryChapter: 10, activationCost: 50, ct: ct);

        // -------------------------------------------------------------------
        // Attributes
        // -------------------------------------------------------------------
        // Kim Dokja's exclusive attribute is given an opaque slug because the
        // canonical name "Fourth Wall" is not spoken on-page until much later.
        // The page title ("Unknown Exclusive Attribute") is the spoiler-safe
        // form a ch-5 reader sees; the real name is revealed inline at ch 51.
        var attrFourthWall = await UpsertAttributeAsync(db,
            slug: "attribute-dokja-exclusive",
            name: "Unknown Exclusive Attribute",
            rarity: AttributeRarity.Mythic,
            effect:
                "Kim Dokja's exclusive attribute, awarded at the start of the scenarios by an unknown " +
                "sender via the gift file from author tls123. Its Attribute Window cannot be opened by " +
                "the bearer — even Kim Dokja cannot read what the system says about it.\n\n" +
                "[spoiler ch=51]Its true name is *The Fourth Wall*. A walled-off layer of cognition that " +
                "makes Dokja unreadable to the system and to other psychics. It blocks the Attribute " +
                "Window, dampens emotional pain, and over time develops a will of its own.[/spoiler]",
            discoveryChapter: 5, ct: ct);

        var attrPrepSpec = await UpsertAttributeAsync(db,
            slug: "attribute-preparation-specialist",
            name: "Preparation Specialist",
            rarity: AttributeRarity.Rare,
            effect:
                "An attribute that reduces the effort and resources required to plan ahead. Allows the " +
                "bearer to derive disproportionate value from items they have already gathered.",
            discoveryChapter: 12, ct: ct);

        // -------------------------------------------------------------------
        // Skills
        // -------------------------------------------------------------------
        var skillORV = await UpsertSkillAsync(db,
            slug: "skill-omniscient-readers-viewpoint",
            name: "Omniscient Reader's Viewpoint",
            type: SkillType.Active,
            effect:
                "Kim Dokja's exclusive skill. Reveals the inner monologue of any character whose original " +
                "story Dokja has read. At Lv. 1 the skill fails on characters not registered in the " +
                "*Ways of Survival* canon (e.g. Yoo Sangah, Han Myungoh) and on entities lacking " +
                "consciousness (such as demonic persons).",
            discoveryChapter: 10, costInCoins: null, maxLevel: 10, ct: ct);

        var skillCharList = await UpsertSkillAsync(db,
            slug: "skill-character-list",
            name: "Character List",
            type: SkillType.Active,
            effect:
                "Reads the basic information of any character originally written into *Ways of Survival*. " +
                "Returns nothing for off-canon characters such as Yoo Sangah and Han Myungoh.",
            discoveryChapter: 10, costInCoins: null, ct: ct);

        var skillBookmark = await UpsertSkillAsync(db,
            slug: "skill-bookmark",
            name: "Bookmark",
            type: SkillType.Active,
            effect:
                "Marks a character so that a portion of their growth can be observed and partially " +
                "shared with the user. Acquired by Kim Dokja from his exclusive skill slot during the " +
                "ichthyosaur arc.",
            discoveryChapter: 12, costInCoins: 1000, ct: ct);

        var skillLieDetect = await UpsertSkillAsync(db,
            slug: "skill-lie-detection",
            name: "Lie Detection",
            type: SkillType.Passive,
            effect: "Notifies the user when an interlocutor is knowingly speaking falsely.",
            discoveryChapter: 13, costInCoins: 200, ct: ct);

        var skillDiverseComm = await UpsertSkillAsync(db,
            slug: "skill-diverse-communication",
            name: "Diverse Communication",
            type: SkillType.Active,
            effect:
                "Lee Gilyoung's exclusive skill. Allows him to mentally direct insects and other small " +
                "creatures, scouting through them and overwhelming foes with swarms. Carries feedback " +
                "damage when his proxies are killed.",
            discoveryChapter: 19, costInCoins: null, maxLevel: 10, ct: ct);

        // -------------------------------------------------------------------
        // Items
        // -------------------------------------------------------------------
        var itemKnife = await UpsertItemAsync(db,
            slug: "item-swiss-army-knife",
            name: "Swiss Army Knife",
            grade: ItemGrade.Common,
            description:
                "A small folding knife carried by the late Kim Namwoon. Recovered by Kim Dokja after " +
                "the murderer scenario; his only weapon for most of Arc 1.",
            discoveryChapter: 7, ct: ct);

        var itemThorn = await UpsertItemAsync(db,
            slug: "item-stone-hog-thorn",
            name: "Stone Hog's Thorn",
            grade: ItemGrade.Uncommon,
            description:
                "A spine harvested from a coastal stone-hog. Sharp enough to pierce the stomach lining " +
                "of an ichthyosaur and emits a faint glow that condenses the prey's life force. Kim " +
                "Dokja exploits it as both a wound-plug and a slow drain on the ichthyosaur's vitality.",
            discoveryChapter: 12, ct: ct);

        var itemMucus = await UpsertItemAsync(db,
            slug: "item-hammer-seahorse-mucus",
            name: "Hammer Seahorse Mucus",
            grade: ItemGrade.Uncommon,
            description:
                "Mucus secreted by the hammer seahorse, evolved as a defence against being digested by " +
                "ichthyosaurs. Coated on the skin it neutralises the digestive juices of the predator.",
            discoveryChapter: 12, ct: ct);

        // -------------------------------------------------------------------
        // Locations
        // -------------------------------------------------------------------
        // The structural Dimension column is a plain string (not RenderedContent),
        // so we keep it spoiler-safe. "Earth · 1864th worldline" pre-spoils the
        // regression count to a ch-1 reader; we use the neutral form instead.
        var locSeoul = await UpsertLocationAsync(db,
            slug: "loc-seoul",
            name: "Seoul",
            discoveryChapter: 1,
            dimension: "Earth",
            worldlineId: mainLine.Id,
            description:
                "Capital of South Korea and the opening stage of the new world's scenarios. Most of " +
                "Arc 1 takes place along the Han River corridor between Geumho Station and the south bank.",
            ct: ct);

        var locCarriage = await UpsertLocationAsync(db,
            slug: "loc-subway-carriage-3807",
            name: "Subway Carriage 3807",
            discoveryChapter: 2,
            dimension: "Earth",
            worldlineId: mainLine.Id, parentLocationId: locSeoul.Id,
            description:
                "The fifth car of the 3434 train to Bulgwang. Five passengers survived: Kim Dokja, " +
                "Lee Hyunsung, Yoo Sangah, Han Myungoh and Lee Gilyoung. The carriage immediately ahead " +
                "(3707) carried Yoo Junghyuk, whose lighter ended every other survivor on his train.",
            ct: ct);

        var locDongho = await UpsertLocationAsync(db,
            slug: "loc-dongho-bridge",
            name: "Dongho Bridge",
            discoveryChapter: 7,
            dimension: "Earth",
            worldlineId: mainLine.Id, parentLocationId: locSeoul.Id,
            description:
                "The bridge across the Han River where the carriage stops. Site of the *Cross the Bridge* " +
                "scenario, the demonic-person ambush, and Kim Dokja's leap into the ichthyosaur's mouth.",
            ct: ct);

        var locDarkRoot = await UpsertLocationAsync(db,
            slug: "loc-dark-root",
            name: "Dark Root",
            discoveryChapter: 17,
            dimension: "Earth · subterranean",
            worldlineId: mainLine.Id, parentLocationId: locSeoul.Id,
            description:
                "An underground root-system burned by black ether, riddled with ground-rat tunnels and " +
                "lit by black flames. The Dark Keeper presides at its core. Site of the climactic " +
                "encounter that closes Arc 1.",
            ct: ct);

        // -------------------------------------------------------------------
        // Scenarios
        // -------------------------------------------------------------------
        var scDisclaimer = await UpsertScenarioAsync(db,
            slug: "sc-disclaimer",
            code: "M-0", title: "Disclaimer",
            type: ScenarioType.Main, difficulty: ScenarioDifficulty.F,
            chapterStart: 4, chapterEnd: 4,
            conditions: "Kill one cockroach within five minutes.",
            rewards: "100 coins. Acceptance into the scenario system.",
            penalty: "Death.",
            ct: ct);
        await LinkScenarioLocationAsync(db, scDisclaimer.Id, locCarriage.Id, ct);

        var scMurderer = await UpsertScenarioAsync(db,
            slug: "sc-murderer",
            code: "M-1", title: "Murderer",
            type: ScenarioType.Main, difficulty: ScenarioDifficulty.D,
            chapterStart: 6, chapterEnd: 6,
            conditions: "Kill one fellow passenger within thirty minutes.",
            rewards: "300 coins per kill. Survival of the carriage.",
            penalty: "Death of every passenger if no kill is made.",
            ct: ct);
        await LinkScenarioLocationAsync(db, scMurderer.Id, locCarriage.Id, ct);

        var scBridge = await UpsertScenarioAsync(db,
            slug: "sc-cross-the-bridge",
            code: "M-2", title: "Cross the Bridge",
            type: ScenarioType.Main, difficulty: ScenarioDifficulty.C,
            chapterStart: 8, chapterEnd: 11,
            conditions: "Cross Dongho Bridge to the south bank within fifteen minutes.",
            rewards: "500 coins on success.",
            penalty:
                "Anyone left on the bridge when the count of survivors becomes odd is dropped into the " +
                "Han River, where they are eaten by an ichthyosaur.",
            ct: ct);
        await LinkScenarioLocationAsync(db, scBridge.Id, locDongho.Id, ct);

        var scIchthyo = await UpsertScenarioAsync(db,
            slug: "sc-slay-the-ichthyosaur",
            code: "H-1", title: "Slay the Ichthyosaur from Within",
            type: ScenarioType.Hidden, difficulty: ScenarioDifficulty.A,
            chapterStart: 11, chapterEnd: 15,
            conditions: "Be swallowed alive by an ichthyosaur and kill it from inside the digestive tract.",
            rewards: "9,000 coins. First-clear bonus 1,000 coins. Permanent Physique stat boost.",
            penalty: "Death by digestion.",
            ct: ct);
        await LinkScenarioLocationAsync(db, scIchthyo.Id, locDongho.Id, ct);

        var scDarkKeeper = await UpsertScenarioAsync(db,
            slug: "sc-kill-the-guard",
            code: "S-1", title: "Kill the Guard",
            type: ScenarioType.Sub, difficulty: ScenarioDifficulty.B,
            chapterStart: 22, chapterEnd: 23,
            conditions: "Defeat the Dark Keeper presiding over the core of Dark Root.",
            rewards: "Variable coins. Black-ether residue. Access to the next scenario region.",
            penalty: "Death.",
            ct: ct);
        await LinkScenarioLocationAsync(db, scDarkKeeper.Id, locDarkRoot.Id, ct);

        // -------------------------------------------------------------------
        // Characters (with their constellation/skill/attribute/item links)
        // -------------------------------------------------------------------

        // ---- Kim Dokja ----
        // Visible from ch 1. The visible portion stays at "what we know on day
        // one." Each later beat (Namwoon, ichthyosaur, Dark Root, Fourth Wall)
        // is gated to its own chapter.
        var kimDokja = await UpsertCharacterAsync(db,
            slug: "kim-dokja", fullName: "Kim Dokja", discoveryChapter: 1,
            shortDescription:
                "Twenty-eight-year-old contractor at a large subsidiary. The sole consistent reader of " +
                "an obscure web novel called *Three Ways to Survive in a Ruined World*.",
            configure: c =>
            {
                c.Status = CharacterStatus.Alive;
                c.Gender = Gender.Male;
                c.Species = "human";
                c.Alias = null;
                c.BirthChapter = 1;
                c.Biography =
                    "Kim Dokja is a 28-year-old single man working as a contractor at a large " +
                    "subsidiary, with no notable career or social life. His only constant for over a " +
                    "decade has been *Three Ways to Survive in a Ruined World*, an obscure 3,149-chapter " +
                    "web novel by an author writing under the pen name **tls123**.\n\n" +
                    "[spoiler ch=3]On the evening of the novel's final chapter the world becomes the " +
                    "novel. A dokkaebi opens a channel above his subway carriage and the scenarios " +
                    "begin. Kim Dokja is suddenly the only person on Earth who has read the script.[/spoiler]\n\n" +
                    "[spoiler ch=6]He uses that knowledge ruthlessly. To break the murderer scenario " +
                    "without sacrificing Yoo Sangah or Lee Gilyoung, he kills Kim Namwoon himself.[/spoiler]\n\n" +
                    "[spoiler ch=15]To open a hidden scenario he leaps into the mouth of an ichthyosaur " +
                    "and slays it from inside its stomach across four days, banking the coins and the " +
                    "first-clear bonus that fund his early advancement.[/spoiler]\n\n" +
                    "[spoiler ch=23]At the close of Episode 1 he leads the survivors of carriage 3807 " +
                    "into Dark Root, exposes Han Myungoh, and brings down the Dark Keeper.[/spoiler]\n\n" +
                    "[spoiler ch=51]His exclusive attribute — granted by the author of *Ways of Survival* " +
                    "— is *The Fourth Wall*: a walled-off layer of cognition that blocks the system's " +
                    "Attribute Window and dampens his emotions, which is part of why he can plan so " +
                    "coldly through atrocity.[/spoiler]";
            }, ct: ct);

        await LinkCharacterAttributeAsync(db, kimDokja.Id, attrFourthWall.Id, acquiredChapter: 5, ct);
        await LinkCharacterAttributeAsync(db, kimDokja.Id, attrPrepSpec.Id, acquiredChapter: 12, ct);
        await LinkCharacterSkillAsync(db, kimDokja.Id, skillORV.Id, level: 1, acquiredChapter: 5, ct);
        await LinkCharacterSkillAsync(db, kimDokja.Id, skillCharList.Id, level: 1, acquiredChapter: 5, ct);
        await LinkCharacterSkillAsync(db, kimDokja.Id, skillBookmark.Id, level: 1, acquiredChapter: 12, ct);
        await LinkCharacterSkillAsync(db, kimDokja.Id, skillLieDetect.Id, level: 1, acquiredChapter: 13, ct);
        await LinkCharacterItemAsync(db, kimDokja.Id, itemKnife.Id, acquiredChapter: 7, lostChapter: null, ct);
        await LinkCharacterItemAsync(db, kimDokja.Id, itemThorn.Id, acquiredChapter: 11, lostChapter: 22, ct);
        await LinkCharacterItemAsync(db, kimDokja.Id, itemMucus.Id, acquiredChapter: 11, lostChapter: 15, ct);
        await LinkCharacterConstellationAsync(db, kimDokja.Id, golHeadband.Id,
            CharacterConstellationRel.MainSponsor, sinceChapter: 22, ct);
        await LinkCharacterConstellationAsync(db, kimDokja.Id, demonJudge.Id,
            CharacterConstellationRel.Patron, sinceChapter: 10, ct);
        await LinkCharacterConstellationAsync(db, kimDokja.Id, secretivePlotter.Id,
            CharacterConstellationRel.Subscriber, sinceChapter: 10, ct);
        await LinkCharacterConstellationAsync(db, kimDokja.Id, blackFlameDragon.Id,
            CharacterConstellationRel.Opposed, sinceChapter: 22, ct);
        await LinkScenarioParticipantAsync(db, scDisclaimer.Id, kimDokja.Id, ScenarioOutcome.Succeeded, ct);
        await LinkScenarioParticipantAsync(db, scMurderer.Id, kimDokja.Id, ScenarioOutcome.Succeeded, ct);
        await LinkScenarioParticipantAsync(db, scBridge.Id, kimDokja.Id, ScenarioOutcome.Succeeded, ct);
        await LinkScenarioParticipantAsync(db, scIchthyo.Id, kimDokja.Id, ScenarioOutcome.Succeeded, ct);
        await LinkScenarioParticipantAsync(db, scDarkKeeper.Id, kimDokja.Id, ScenarioOutcome.Succeeded, ct);
        await LinkPageTagAsync(db, kimDokja.PageId, tagProtagonist.Id, ct);
        await LinkPageTagAsync(db, kimDokja.PageId, tagIncarnation.Id, ct);
        await LinkPageTagAsync(db, kimDokja.PageId, tagArc1.Id, ct);

        // ---- Yoo Junghyuk ----
        // Visible from ch 5. A ch-5 reader knows him only as "the cold guy in
        // the next carriage who handled the situation calmly." Recognition by
        // Kim Dokja and the regressor concept land in ch 9–11. The specific
        // numbered round (1864/1863) is much later and is gated separately.
        var yooJunghyuk = await UpsertCharacterAsync(db,
            slug: "yoo-junghyuk", fullName: "Yoo Junghyuk", discoveryChapter: 5,
            shortDescription:
                "A passenger in subway carriage 3707, immediately ahead of Kim Dokja's. Composed and " +
                "lethally efficient when the scenarios begin.",
            configure: c =>
            {
                c.Status = CharacterStatus.Alive;
                c.Gender = Gender.Male;
                c.Species = "human";
                c.Alias = null;
                c.BirthChapter = 5;
                c.Biography =
                    "Yoo Junghyuk is the unnamed young man in carriage 3707 who steps off the train " +
                    "with a pocket lighter on the night the dokkaebi appear. Where the rest of the city " +
                    "panics, he is unhurried, almost bored, as he handles the demonic surge on Dongho " +
                    "Bridge.\n\n" +
                    "Kim Dokja recognises him immediately — though he does not return the recognition. " +
                    "Their first meeting is brief, and Junghyuk vanishes into the south-bank wreckage " +
                    "before any conversation can take place.\n\n" +
                    "[spoiler ch=11]He is the protagonist of the in-universe web novel *Three Ways to " +
                    "Survive in a Ruined World* — the canonical hero whose decisions Kim Dokja has been " +
                    "reading for over a decade.[/spoiler]\n\n" +
                    "[spoiler ch=11]Junghyuk is a **regressor**. When he dies, he returns to the start " +
                    "of the scenarios. The composure that strikes the other passengers as inhuman is " +
                    "simply the calm of someone who has done all of this before. Kim Dokja deduces it " +
                    "during their first conversation in the demonic-person ambush.[/spoiler]\n\n" +
                    "[spoiler ch=266]Specifically, he is on the 1864th iteration; the canonical *Ways " +
                    "of Survival* documents the 1863rd. Before this story ends he will have lived the " +
                    "apocalypse hundreds of times.[/spoiler]\n\n" +
                    "[spoiler ch=51]He carries the exclusive skill *Returning to the Beginning* and is " +
                    "the wielder of the Black Heaven & Earth, the sword passed down through every one " +
                    "of his lives.[/spoiler]";
            }, ct: ct);
        await LinkPageTagAsync(db, yooJunghyuk.PageId, tagIncarnation.Id, ct);
        await LinkPageTagAsync(db, yooJunghyuk.PageId, tagArc1.Id, ct);

        // ---- Yoo Sangah ----
        // Visible from ch 2. The "off-canon" status is only revealed ch 10
        // (when Dokja's Character List skill returns nothing for her), and
        // her abduction / rescue are ch 10 / ch 22.
        var yooSangah = await UpsertCharacterAsync(db,
            slug: "yoo-sangah", fullName: "Yoo Sangah", discoveryChapter: 2,
            shortDescription:
                "Kim Dokja's colleague from the HR team at his subsidiary. Studies foreign languages on " +
                "her commute home.",
            configure: c =>
            {
                c.Status = CharacterStatus.Alive;
                c.Gender = Gender.Female;
                c.Species = "human";
                c.BirthChapter = 2;
                c.Biography =
                    "Yoo Sangah works in the human-resources team at the same subsidiary as Kim Dokja. " +
                    "She studies foreign languages on her commute and, until her bicycle was stolen " +
                    "weeks earlier, rode it to work. Their conversation on the evening line-3 train is " +
                    "the only meaningful exchange they have ever had.\n\n" +
                    "[spoiler ch=10]She is not a character in the canonical *Ways of Survival* — Kim " +
                    "Dokja's *Character List* skill returns no entry for her — and would have died in " +
                    "carriage 3807 had he not deliberately altered the script.[/spoiler]\n\n" +
                    "[spoiler ch=10]On Dongho Bridge she is abducted by Han Myungoh, who has just " +
                    "received a stigma from the Lame Trickster, and is dragged into Dark Root.[/spoiler]\n\n" +
                    "[spoiler ch=22]She is recovered alive at the climax of the Dark Keeper encounter " +
                    "after Kim Dokja exposes her abductor's pre-apocalypse grudge against her.[/spoiler]";
            }, ct: ct);
        await LinkPageTagAsync(db, yooSangah.PageId, tagArc1.Id, ct);
        await LinkScenarioParticipantAsync(db, scDisclaimer.Id, yooSangah.Id, ScenarioOutcome.Succeeded, ct);
        await LinkScenarioParticipantAsync(db, scMurderer.Id, yooSangah.Id, ScenarioOutcome.Succeeded, ct);

        // ---- Lee Hyunsung ----
        // Visible from ch 5. The meta-narrative "canonical survivor" detail
        // is something Kim Dokja's POV reveals over time and is itself a
        // mid-novel structural reveal.
        var leeHyunsung = await UpsertCharacterAsync(db,
            slug: "lee-hyunsung", fullName: "Lee Hyunsung", discoveryChapter: 5,
            shortDescription:
                "Tall, soft-spoken man with the build and reflexes of a career soldier. Fellow passenger " +
                "of subway carriage 3807 on the night the scenarios begin.",
            configure: c =>
            {
                c.Status = CharacterStatus.Alive;
                c.Gender = Gender.Male;
                c.Species = "human";
                c.BirthChapter = 5;
                c.Biography =
                    "Lee Hyunsung has the build and reflexes of a career soldier. He is the only adult " +
                    "in carriage 3807 who clearly *could* stop Kim Namwoon's mob — and the visible " +
                    "trembling of his fists during the murderer scenario tells Kim Dokja he is the kind " +
                    "of man who will pay for inaction the rest of his life.\n\n" +
                    "[spoiler ch=11]In the canonical *Ways of Survival* Lee Hyunsung is one of only two " +
                    "passengers in carriage 3807 that Yoo Junghyuk spares. Kim Dokja knows this from " +
                    "the start, which is why he treats Hyunsung's survival as a foregone conclusion " +
                    "rather than something to engineer.[/spoiler]\n\n" +
                    "[spoiler ch=23]When the script diverges he survives anyway and joins the party " +
                    "that descends into Dark Root. His moral discomfort during the murderer scenario " +
                    "is the first sign of what will become an unshakeable long-term loyalty to Kim " +
                    "Dokja.[/spoiler]";
            }, ct: ct);
        await LinkPageTagAsync(db, leeHyunsung.PageId, tagIncarnation.Id, ct);
        await LinkPageTagAsync(db, leeHyunsung.PageId, tagArc1.Id, ct);
        await LinkScenarioParticipantAsync(db, scDisclaimer.Id, leeHyunsung.Id, ScenarioOutcome.Succeeded, ct);
        await LinkScenarioParticipantAsync(db, scMurderer.Id, leeHyunsung.Id, ScenarioOutcome.Succeeded, ct);
        await LinkScenarioParticipantAsync(db, scBridge.Id, leeHyunsung.Id, ScenarioOutcome.Succeeded, ct);

        // ---- Lee Gilyoung ----
        // Visible from ch 4. His exclusive skill is only revealed in ch 19.
        var leeGilyoung = await UpsertCharacterAsync(db,
            slug: "lee-gilyoung", fullName: "Lee Gilyoung", discoveryChapter: 4,
            shortDescription:
                "A small boy travelling on subway carriage 3807 with his mother on the night the " +
                "scenarios begin.",
            configure: c =>
            {
                c.Status = CharacterStatus.Alive;
                c.Gender = Gender.Male;
                c.Species = "human";
                c.BirthChapter = 4;
                c.Biography =
                    "Lee Gilyoung is a young boy travelling with his mother in carriage 3807. He " +
                    "struggles to complete the disclaimer scenario — killing a cockroach is not in " +
                    "his vocabulary — and Kim Dokja crouches down and quietly helps him through it.\n\n" +
                    "[spoiler ch=6]His mother is killed by Kim Namwoon's mob during the murderer " +
                    "scenario; Gilyoung watches her die without intervening. Kim Dokja takes the boy " +
                    "under his protection afterwards, with the cold internal observation that doing " +
                    "so will please the watching constellations.[/spoiler]\n\n" +
                    "[spoiler ch=19]In Dark Root, Gilyoung manifests his exclusive skill *Diverse " +
                    "Communication* — mental control over insects and other small creatures. He scouts " +
                    "ahead through the cockroach swarm despite the feedback damage of having his proxies " +
                    "killed.[/spoiler]";
            }, ct: ct);
        await LinkCharacterSkillAsync(db, leeGilyoung.Id, skillDiverseComm.Id,
            level: 1, acquiredChapter: 19, ct);
        await LinkPageTagAsync(db, leeGilyoung.PageId, tagIncarnation.Id, ct);
        await LinkPageTagAsync(db, leeGilyoung.PageId, tagArc1.Id, ct);
        await LinkScenarioParticipantAsync(db, scDisclaimer.Id, leeGilyoung.Id, ScenarioOutcome.Succeeded, ct);
        await LinkScenarioParticipantAsync(db, scMurderer.Id, leeGilyoung.Id, ScenarioOutcome.Succeeded, ct);
        await LinkScenarioParticipantAsync(db, scBridge.Id, leeGilyoung.Id, ScenarioOutcome.Succeeded, ct);
        await LinkScenarioParticipantAsync(db, scDarkKeeper.Id, leeGilyoung.Id, ScenarioOutcome.Succeeded, ct);

        // ---- Han Myungoh ----
        // Visible from ch 5. Status/DeathChapter on the entity stay at "Alive /
        // null" so the page metadata that surfaces to a ch-5 reader does not
        // pre-spoil his fate at the Dark Keeper. The death is described inside
        // the biography behind a [spoiler ch=23] block.
        var hanMyungoh = await UpsertCharacterAsync(db,
            slug: "han-myungoh", fullName: "Han Myungoh", discoveryChapter: 5,
            shortDescription:
                "Middle-aged office worker, fellow passenger of subway carriage 3807.",
            configure: c =>
            {
                c.Status = CharacterStatus.Alive;
                c.Gender = Gender.Male;
                c.Species = "human";
                c.BirthChapter = 5;
                c.DeathChapter = null;
                c.Biography =
                    "Han Myungoh is the heavy-set office worker who first appears to confront Kim Namwoon " +
                    "in carriage 3807, then immediately backs down once Namwoon turns the carriage to " +
                    "murder. He survives the disclaimer and the murderer scenario by being neither the " +
                    "first to act nor the last to react.\n\n" +
                    "[spoiler ch=10]On Dongho Bridge he receives the *One-legged Swift Horse* stigma " +
                    "from an obscure constellation called the Lame Trickster, abducts Yoo Sangah, and " +
                    "flees south at sprinter speed.[/spoiler]\n\n" +
                    "[spoiler ch=22]At the climax of Episode 5 the party finds him deep in Dark Root " +
                    "alongside the captive Yoo Sangah. Trying to deflect blame, he lets slip that he was " +
                    "the one who stole Yoo Sangah's bicycle in the days before the apocalypse — petty " +
                    "revenge for being refused a ride home. The reveal is what finally provokes Kim " +
                    "Dokja to draw the Dark Keeper out of hiding.[/spoiler]\n\n" +
                    "[spoiler ch=23]Han Myungoh does not survive the Dark Keeper encounter that closes " +
                    "Arc 1.[/spoiler]";
            }, ct: ct);
        await LinkCharacterStigmaAsync(db, hanMyungoh.Id,
            stigmaId: (await db.Stigmas.FirstAsync(s => s.Name == "One-legged Swift Horse", ct)).Id,
            isPrimary: true, acquiredChapter: 10, ct);
        await LinkCharacterConstellationAsync(db, hanMyungoh.Id, lameTrickster.Id,
            CharacterConstellationRel.Patron, sinceChapter: 10, ct);
        await LinkPageTagAsync(db, hanMyungoh.PageId, tagIncarnation.Id, ct);
        await LinkPageTagAsync(db, hanMyungoh.PageId, tagArc1.Id, ct);
        await LinkScenarioParticipantAsync(db, scDisclaimer.Id, hanMyungoh.Id, ScenarioOutcome.Succeeded, ct);
        await LinkScenarioParticipantAsync(db, scMurderer.Id, hanMyungoh.Id, ScenarioOutcome.Succeeded, ct);

        // ---- Kim Namwoon ----
        // Visible from ch 5; he dies in ch 6. Structural status/death-chapter
        // on the entity stay at "Alive / null" so a ch-5 reader's profile view
        // does not flash a one-chapter-early death; the actual death is in the
        // biography behind a [spoiler ch=6] block.
        var kimNamwoon = await UpsertCharacterAsync(db,
            slug: "kim-namwoon", fullName: "Kim Namwoon", discoveryChapter: 5,
            shortDescription:
                "Thin, white-haired student in a high-school uniform. Travelling alone in subway " +
                "carriage 3807 on the night the scenarios begin.",
            configure: c =>
            {
                c.Status = CharacterStatus.Alive;
                c.Gender = Gender.Male;
                c.Species = "human";
                c.BirthChapter = 5;
                c.DeathChapter = null;
                c.Biography =
                    "Kim Namwoon is a thin, white-haired student travelling alone in carriage 3807. " +
                    "He is the first passenger to grasp that the holographic broadcast on the ceiling — " +
                    "a live feed of mass killings across the country — is real, and the first to act on " +
                    "it: beating an elderly woman to death and rallying the carriage to cross the " +
                    "murder threshold of scenario M-1.\n\n" +
                    "[spoiler ch=6]Kim Dokja kills him with a Swiss Army knife to break the murderer " +
                    "scenario in his own way, rather than let the rest of the carriage do it.[/spoiler]\n\n" +
                    "[spoiler ch=8]On Dongho Bridge Namwoon's corpse re-animates as a 9th-grade demonic " +
                    "person and pursues Kim Dokja across the bridge before finally being put down.[/spoiler]\n\n" +
                    "[spoiler ch=11]In the canonical *Ways of Survival* Kim Namwoon is, alongside Lee " +
                    "Hyunsung, one of only two passengers Yoo Junghyuk spares from carriage 3807 — " +
                    "which means Dokja's choice to kill him in chapter 6 is the first hard divergence " +
                    "between the new world and the script he has read.[/spoiler]";
            }, ct: ct);
        await LinkPageTagAsync(db, kimNamwoon.PageId, tagArc1.Id, ct);
        await LinkScenarioParticipantAsync(db, scDisclaimer.Id, kimNamwoon.Id, ScenarioOutcome.Succeeded, ct);
        await LinkScenarioParticipantAsync(db, scMurderer.Id, kimNamwoon.Id, ScenarioOutcome.Failed, ct);

        // ---- tls123 (the pen name; visible from ch 1) ----
        // CAREFUL: this page is visible from ch 1. The biography stays strictly
        // at what a chapter-1 reader knows — a username, a thank-you message,
        // a gift file. The identity behind the pen name is a separate Character
        // row (`han-sooyoung`, ch 68 discovery) and the connection between them
        // is gated to ch 533 on *that* page. Nothing about Han Sooyoung is
        // mentioned here, even behind a spoiler tag, so that the visible page
        // does not pre-name a character the reader has not met.
        var tls123 = await UpsertCharacterAsync(db,
            slug: "tls123", fullName: "tls123", discoveryChapter: 1,
            shortDescription:
                "An anonymous web-novel author. Pen name of the writer of *Three Ways to Survive in a " +
                "Ruined World* — and the only person who ever messaged Kim Dokja about it.",
            configure: c =>
            {
                // Deliberately leave species/gender/alias unset — even "human"
                // or "female" pre-spoils the eventual identity reveal.
                c.Status = CharacterStatus.Unknown;
                c.Gender = null;
                c.Species = null;
                c.Alias = null;
                c.BirthChapter = 1;
                c.Biography =
                    "tls123 is the username under which an unidentified author serialised the 3,149-chapter " +
                    "web novel *Three Ways to Survive in a Ruined World* on a small Korean web-novel " +
                    "platform. For ten years the novel had effectively one consistent reader: Kim Dokja.\n\n" +
                    "On the day the work completes, tls123 messages Kim Dokja to thank him and sends a " +
                    "gift file by email — the file that, hours later, grants Dokja his exclusive " +
                    "attribute and skill slot. The conversation ends abruptly and the author goes silent.\n\n" +
                    "Across the early scenarios Kim Dokja occasionally receives further text files from " +
                    "the same address; the system attributes them to the gift's original sender. The " +
                    "identity behind the pen name is not discussed in *Ways of Survival* itself and " +
                    "does not surface in the carriage-3807 storyline.";
            }, ct: ct);
        await LinkPageTagAsync(db, tls123.PageId, tagArc1.Id, ct);

        // ---- Han Sooyoung (forward-declared; visible from ch 68) ----
        // Han Sooyoung is introduced in Episode 14 (Master of the Throne)
        // around ch 68, when she introduces herself to Kim Dokja as
        // Cha Sangkyung's assistant in the Big Dipper waiting room. Her page
        // therefore stays hidden until then. The connection between her and
        // tls123 — i.e. the fact that she IS the author Kim Dokja read for ten
        // years — is gated to ch 533, the chapter where the canonical text
        // states it explicitly ("Han Sooyoung was tls123"). Until that chapter
        // the two characters appear as wholly separate entities in the wiki.
        var hanSooyoung = await UpsertCharacterAsync(db,
            slug: "han-sooyoung", fullName: "Han Sooyoung", discoveryChapter: 68,
            shortDescription:
                "An incarnation Kim Dokja first encounters in the Big Dipper waiting room, where she " +
                "introduces herself as the assistant to the king Cha Sangkyung.",
            configure: c =>
            {
                c.Status = CharacterStatus.Alive;
                c.Gender = Gender.Female;
                c.Species = "human";
                c.Alias = null;
                c.BirthChapter = 68;
                c.Biography =
                    "Han Sooyoung first appears in Episode 14 in the Big Dipper waiting room, " +
                    "presenting herself to Kim Dokja as the assistant to a king named Cha Sangkyung. " +
                    "Her bearing strikes him as off — too theatrical, too quick to drop literary " +
                    "references — and his *Lie Detection* skill flickers through several of her " +
                    "lines.\n\n" +
                    "[spoiler ch=70]She is herself an incarnation, and an aspiring novelist by " +
                    "background. Across the Big Dipper chapter she manoeuvres Kim Dokja into " +
                    "alliances, betrayals, and side-deals with kings — the kind of plotting that " +
                    "comes naturally to a writer used to reshuffling characters on a page.[/spoiler]\n\n" +
                    "[spoiler ch=533]Han Sooyoung is the author behind the pen name **tls123**. The " +
                    "3,149-chapter web novel Kim Dokja read for over a decade was written by her — " +
                    "the same person who would later sit beside him in the carriage-3807 storyline. " +
                    "The Dokkaebi King's research into the author of *Ways of Survival* converges on " +
                    "her in Episode 99 (the Oldest Dream chapter), and the canonical text states the " +
                    "identification outright in Epilogue 4.[/spoiler]";
            }, ct: ct);
        await LinkPageTagAsync(db, hanSooyoung.PageId, tagIncarnation.Id, ct);

        // -------------------------------------------------------------------
        // Events (key beats for the 3D timeline)
        // -------------------------------------------------------------------
        var ev1 = await UpsertEventAsync(db,
            slug: "ev-novel-completes",
            title: "Ways of Survival completes its run",
            chapterNumber: 1, importance: EventImportance.Pivotal,
            description:
                "After more than a decade, *Three Ways to Survive in a Ruined World* posts its 3,149th " +
                "chapter. Kim Dokja submits a recommendation; tls123 messages him a thank-you and sends " +
                "the gift file that will become Kim Dokja's exclusive attribute and skill slot.",
            eventOrder: 1, worldlineId: mainLine.Id, ct: ct);
        await LinkEventCharacterAsync(db, ev1.Id, kimDokja.Id, EventCharacterRole.Participant, ct);
        await LinkEventCharacterAsync(db, ev1.Id, tls123.Id, EventCharacterRole.Mentor, ct);

        var ev2 = await UpsertEventAsync(db,
            slug: "ev-bihyung-opens-channel",
            title: "Bihyung opens channel #BI-7623 above carriage 3807",
            chapterNumber: 3, importance: EventImportance.Pivotal,
            description:
                "Bihyung the dokkaebi enters carriage 3807 at 7 p.m., the moment monetisation begins. " +
                "When passengers protest, he bursts their heads — the first deaths in the new world for " +
                "Kim Dokja's group. Channel #BI-7623 goes live.",
            eventOrder: 1, locationId: locCarriage.Id, worldlineId: mainLine.Id, ct: ct);
        await LinkEventCharacterAsync(db, ev2.Id, bihyung.Id, EventCharacterRole.Perpetrator, ct);
        await LinkEventCharacterAsync(db, ev2.Id, kimDokja.Id, EventCharacterRole.Observer, ct);

        var ev3 = await UpsertEventAsync(db,
            slug: "ev-dokja-receives-attribute",
            title: "Kim Dokja receives his exclusive attribute and skill slot",
            chapterNumber: 5, importance: EventImportance.Pivotal,
            description:
                "Kim Dokja runs the gift file from tls123. He receives an exclusive attribute (whose " +
                "window he cannot open) and an exclusive skill slot. Reading speed sharply increases.",
            eventOrder: 1, locationId: locCarriage.Id, worldlineId: mainLine.Id, ct: ct);
        await LinkEventCharacterAsync(db, ev3.Id, kimDokja.Id, EventCharacterRole.Participant, ct);

        var ev4 = await UpsertEventAsync(db,
            slug: "ev-namwoon-killed",
            title: "Kim Dokja kills Kim Namwoon",
            chapterNumber: 6, importance: EventImportance.Major,
            description:
                "To break the murderer scenario without sacrificing Yoo Sangah or Lee Gilyoung, Kim " +
                "Dokja stabs Kim Namwoon with the Swiss Army knife. Two constellations turn hostile; " +
                "others sponsor coins.",
            eventOrder: 1, locationId: locCarriage.Id, worldlineId: mainLine.Id, ct: ct);
        await LinkEventCharacterAsync(db, ev4.Id, kimDokja.Id, EventCharacterRole.Perpetrator, ct);
        await LinkEventCharacterAsync(db, ev4.Id, kimNamwoon.Id, EventCharacterRole.Victim, ct);

        var ev5 = await UpsertEventAsync(db,
            slug: "ev-junghyuk-encounter",
            title: "First encounter with Yoo Junghyuk",
            chapterNumber: 9, importance: EventImportance.Pivotal,
            description:
                "Yoo Junghyuk emerges from carriage 3707 onto Dongho Bridge to clear the demonic-person " +
                "horde. Kim Dokja sees the protagonist of *Ways of Survival* in the flesh for the first " +
                "time; Junghyuk is briefly thrown by the appearance of someone who already knows him.",
            eventOrder: 1, locationId: locDongho.Id, worldlineId: mainLine.Id, ct: ct);
        await LinkEventCharacterAsync(db, ev5.Id, kimDokja.Id, EventCharacterRole.Observer, ct);
        await LinkEventCharacterAsync(db, ev5.Id, yooJunghyuk.Id, EventCharacterRole.Participant, ct);

        var ev6 = await UpsertEventAsync(db,
            slug: "ev-myungoh-abducts-sangah",
            title: "Han Myungoh abducts Yoo Sangah",
            chapterNumber: 10, importance: EventImportance.Major,
            description:
                "Granted the *One-legged Swift Horse* stigma by the Lame Trickster, Han Myungoh carries " +
                "the protesting Yoo Sangah across Dongho Bridge at sprinter speed and disappears toward " +
                "the south bank.",
            eventOrder: 2, locationId: locDongho.Id, worldlineId: mainLine.Id, ct: ct);
        await LinkEventCharacterAsync(db, ev6.Id, hanMyungoh.Id, EventCharacterRole.Perpetrator, ct);
        await LinkEventCharacterAsync(db, ev6.Id, yooSangah.Id, EventCharacterRole.Victim, ct);

        var ev7 = await UpsertEventAsync(db,
            slug: "ev-leap-into-ichthyosaur",
            title: "Kim Dokja leaps into the ichthyosaur",
            chapterNumber: 11, importance: EventImportance.Pivotal,
            description:
                "To open the hidden scenario *Slay the Ichthyosaur from Within*, Kim Dokja drinks " +
                "hammer-seahorse mucus and jumps from Dongho Bridge into the open mouth of an " +
                "ichthyosaur in the Han River.",
            eventOrder: 1, locationId: locDongho.Id, worldlineId: mainLine.Id, ct: ct);
        await LinkEventCharacterAsync(db, ev7.Id, kimDokja.Id, EventCharacterRole.Participant, ct);

        var ev8 = await UpsertEventAsync(db,
            slug: "ev-ichthyosaur-slain",
            title: "Hidden scenario cleared: ichthyosaur slain from within",
            chapterNumber: 15, importance: EventImportance.Major,
            description:
                "After four days inside the beast Kim Dokja kills it with the stone hog's thorn, " +
                "claiming 9,000 coins, a 1,000-coin first-clear bonus, and a permanent rise in his " +
                "Physique stat from Lv. 11 to Lv. 12.",
            eventOrder: 1, locationId: locDongho.Id, worldlineId: mainLine.Id, ct: ct);
        await LinkEventCharacterAsync(db, ev8.Id, kimDokja.Id, EventCharacterRole.Perpetrator, ct);
        await LinkEventCharacterAsync(db, ev8.Id, bihyung.Id, EventCharacterRole.Observer, ct);

        var ev9 = await UpsertEventAsync(db,
            slug: "ev-dark-keeper-appears",
            title: "The Dark Keeper appears in Dark Root",
            chapterNumber: 22, importance: EventImportance.Major,
            description:
                "Kim Dokja exposes Han Myungoh as the bicycle thief and throws his last stone-hog " +
                "thorn into the dark, drawing the boss-class Dark Keeper from the deepest part of " +
                "the root system. The sub-scenario *Kill the Guard* begins.",
            eventOrder: 1, locationId: locDarkRoot.Id, worldlineId: mainLine.Id, ct: ct);
        await LinkEventCharacterAsync(db, ev9.Id, kimDokja.Id, EventCharacterRole.Participant, ct);
        await LinkEventCharacterAsync(db, ev9.Id, hanMyungoh.Id, EventCharacterRole.Victim, ct);
        await LinkEventCharacterAsync(db, ev9.Id, yooSangah.Id, EventCharacterRole.Victim, ct);
        await LinkEventCharacterAsync(db, ev9.Id, leeGilyoung.Id, EventCharacterRole.Participant, ct);

        var ev10 = await UpsertEventAsync(db,
            slug: "ev-arc1-clear",
            title: "Episode 1 of the new world clears",
            chapterNumber: 23, importance: EventImportance.Pivotal,
            description:
                "The Dark Keeper falls. Bihyung confirms the path forward to the next scenario region. " +
                "The first arc of Kim Dokja's altered *Ways of Survival* closes with five passengers " +
                "of carriage 3807 still alive.",
            eventOrder: 1, locationId: locDarkRoot.Id, worldlineId: mainLine.Id, ct: ct);
        await LinkEventCharacterAsync(db, ev10.Id, kimDokja.Id, EventCharacterRole.Participant, ct);
        await LinkEventCharacterAsync(db, ev10.Id, leeHyunsung.Id, EventCharacterRole.Participant, ct);
        await LinkEventCharacterAsync(db, ev10.Id, leeGilyoung.Id, EventCharacterRole.Participant, ct);
        await LinkEventCharacterAsync(db, ev10.Id, yooSangah.Id, EventCharacterRole.Participant, ct);
        await LinkEventCharacterAsync(db, ev10.Id, bihyung.Id, EventCharacterRole.Observer, ct);
    }
}
