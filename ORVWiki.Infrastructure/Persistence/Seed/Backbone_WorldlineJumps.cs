using Microsoft.EntityFrameworkCore;
using static ORVWiki.Infrastructure.Persistence.Seed.SeedHelpers;

namespace ORVWiki.Infrastructure.Persistence.Seed;

/// <summary>
/// Cross-arc backbone: the worldlines that bracket the regression cycle and
/// the canonical worldline-to-worldline jumps the novel actually depicts on
/// page. Runs AFTER Arc01 because Arc01 already creates the 1863rd / 1864th
/// rows the jumps below depend on. Re-running is safe — every helper here is
/// keyed by a natural identifier and upserts.
///
/// Scope (decided with the user, 2026-05-10):
///   • Curated canonical jumps only — not every one of Yoo Joonghyuk's 1863
///     prior regressions. Each row corresponds to a worldline transition that
///     is named *and* shown in the canonical text.
///   • "Outer-world" detours (Cretaceous, Industrial Complex, Greek myth, the
///     10,000-year Sage Grass arc, etc.) are modelled as Events on the 1864th
///     lane, not as separate worldlines. They are sub-scenarios *within* a
///     turn, not regressions of the turn counter.
///
/// Spoiler conventions follow the same rules as Arc01_ThreeWaysToSurvive:
///   1. discoveryChapter = first chapter at which a spoiler-conscious reader
///      could be told the row exists. The page stays hidden before that.
///   2. Substantive narrative facts that go beyond the discovery chapter are
///      wrapped in [spoiler ch=N]…[/spoiler] inside Description.
///   3. The Jump rows themselves do not have their own discoveryChapter — the
///      timeline service gates them by their Arc.ChapterStart, so we attach
///      ArcId only when the jump is actually associated with a specific arc.
///      (For end-game jumps that happen across multiple arcs we leave ArcId
///      null; the visualisation surfaces them once any chapter filter is off.)
/// </summary>
internal static class Backbone_WorldlineJumps
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct)
    {
        // -------------------------------------------------------------------
        // Worldlines (the 3D timeline backbone — the rows Arc01 doesn't own)
        // -------------------------------------------------------------------
        // The 0th turn ("World of Zero" / "First World-line") is the origin
        // worldline before Yoo Joonghyuk's first regression. Mentioned as
        // early as ch 266 inside The Fourth Wall's library ("Yoo Joonghyuk,
        // 56th record of the 0th round."). Its full significance — the place
        // Kim Dokja chooses to live in — is the payoff of Epilogue 1.
        var zerothLine = await UpsertWorldlineAsync(db,
            slug: "0th-worldline", lineNumber: 0,
            name: "0th Worldline", isMain: false, discoveryChapter: 266,
            description:
                "[spoiler ch=266]The origin worldline before the regression cycle began — the " +
                "0th turn (also called the *First World-line* in Han Sooyoung's epilogue). The " +
                "Yoo Joonghyuk of this turn never sponsored a constellation, lost everyone he " +
                "loved, and from his deathbed wished he had a backer; that wish is what set the " +
                "regression cycle in motion.[/spoiler]\n\n" +
                "[spoiler ch=517]After the scenarios end, Kim Dokja splits a 49% Avatar off to live " +
                "with the companions in the 1865th turn and walks his remaining self to the 0th " +
                "turn. From here he watches the original Yoo Joonghyuk's first life unfold, the " +
                "story before the loop. This is the *World of Zero* of Epilogue 1.[/spoiler]",
            // Warm amber for the origin lane — the previous near-black
            // (#0F172A) was invisible against the canvas's dark gradient.
            color: "#F59E0B", displayOrder: 0,
            ct: ct);

        // The 1863rd is owned by Arc01 — re-fetch by slug so we don't try to
        // re-create it and end up with a slug-collision on Page.
        var prevLine = await db.Worldlines
            .FirstAsync(w => w.Page.Slug == "1863rd-worldline", ct);

        var mainLine = await db.Worldlines
            .FirstAsync(w => w.Page.Slug == "1864th-worldline", ct);

        // The 1865th turn appears once Han Sooyoung pays the price to send a
        // version of Kim Dokja's group forward into a still-unwritten future.
        // Number first surfaces when the Secretive Plotter declares his intent
        // to "live through the 1864th turn, and then, the 1865th" (ch 452).
        // The actual landing — Han Sooyoung waking up in 1865th-turn Seoul —
        // is ch 540.
        var nextLine = await UpsertWorldlineAsync(db,
            slug: "1865th-worldline", lineNumber: 1865,
            name: "1865th Worldline", isMain: false, discoveryChapter: 452,
            description:
                "[spoiler ch=452]The first regression turn the Secretive Plotter (Yoo Joonghyuk in " +
                "his Outer-God form) ever named *aloud* as his next destination. Uriel seals him " +
                "before he can reach it personally, but the slot is now part of the cycle.[/spoiler]\n\n" +
                "[spoiler ch=540]The turn Han Sooyoung's *Architect of the False Last Act* " +
                "constellation actually opens. She bleeds out the 1864th-turn ending and the rescued " +
                "child form of Kim Dokja arrives with her in Gwanghwamun, Seoul. The rest of the " +
                "carriage-3807 survivors follow in the chapters after.[/spoiler]",
            // Emerald — slightly brighter than the original (#10B981) so it
            // matches the contrast level of the other lanes.
            color: "#34D399", displayOrder: 3,
            parentWorldlineId: mainLine.Id,
            ct: ct);

        // -------------------------------------------------------------------
        // Worldline jumps (canonical, on-page transitions)
        // -------------------------------------------------------------------
        // Convention for SourceOrder / TargetOrder: a 0-100 abstract "lane
        // position" — 0 is the beginning of that worldline's depicted
        // lifetime, 100 is the end. This is deliberately NOT chapter-
        // number based: chapter numbers belong to the canonical text,
        // which only depicts the 1864th turn directly (with peeks into a
        // few others). Using chapters as positions on the 1863rd or 0th
        // lanes pretends they are partitioned the same way as 1864, which
        // they aren't. Abstract positions sidestep that entirely — they
        // only commit to the relative ordering of jumps along each lane.
        //
        // Rough anchors used below:
        //   0   = before the worldline's depicted events begin
        //   5   = scenarios / story open
        //  50   = mid-lifetime
        //  95   = closing chapters of the worldline
        // 100   = end of the worldline's depicted lifetime

        var arc1 = await db.Arcs.FirstAsync(a => a.Page.Slug == "arc-three-ways-to-survive", ct);

        // (1) Yoo Joonghyuk: 1863rd → 1864th. The novel's foundational jump,
        // but only one of the two paths the 1863rd turn produces.
        //
        // The 1863rd turn forks on whether Han Sooyoung is alongside Yoo
        // Joonghyuk:
        //   • 1863-without-Han-Sooyoung → that Yoo Joonghyuk transcends into
        //     the *Secretive Plotter* (the antagonist Outer God). He does NOT
        //     regress into 1864 — he persists across worldlines as a
        //     constellation. So no Jump row models him; the transformation
        //     happens within the 1863rd lane.
        //   • 1863-with-Han-Sooyoung → that Yoo Joonghyuk acquires a
        //     writer-linked attribute, splits again, and one of those selves
        //     reincarnates into the 1864th-turn subway with only partial
        //     memory of the prior turn. This is the Yoo Joonghyuk the novel
        //     opens with, and it is the path this Jump represents.
        //
        // Source position: he leaves the 1863rd turn at the moment of his
        //   death, mid-to-late in the lane — earlier than Han Sooyoung's
        //   clone, who departs at the very end of the 1863rd turn (after
        //   the events that get written down).
        // Target position: he arrives at the 1864th-turn subway exactly as
        //   the scenarios begin — chapter 1, which we map to lane position
        //   1. Han Sooyoung's clone, by contrast, lands years earlier so
        //   she has time to write *Ways of Survival* before chapter 1.
        // Anchored to Arc 1 because the arrival is Episode 1.
        await UpsertJumpAsync(db,
            characterLabel: "Yoo Joonghyuk (with Han Sooyoung path)",
            sourceWorldlineId: prevLine.Id, sourceOrder: 65d,
            targetWorldlineId: mainLine.Id, targetOrder: 5d,
            description:
                "Of the two paths the 1863rd turn produces, this is the *with Han Sooyoung* " +
                "branch. The Yoo Joonghyuk who lived this turn beside her acquires a writer-" +
                "linked attribute, splits, and one of those selves reincarnates into the " +
                "1864th-turn subway with only partial memory of the prior turn — arriving at " +
                "the moment the scenarios begin (Ep. 1, ch 1), which is where Kim Dokja meets " +
                "him in Episode 2. The other 1863rd-turn Yoo Joonghyuk (the *without Han " +
                "Sooyoung* branch) does not regress; he transcends inside the 1863rd lane and " +
                "becomes the Secretive Plotter.",
            lengthEstimate: "instantaneous (regression with attribute split)",
            arcId: arc1.Id,
            ct: ct);

        // (1b) Han Sooyoung (clone): 1863rd → 1864th, leaving the 1863rd
        // turn at its very end and landing *years* before the events of Arc
        // 1. A clone of Han Sooyoung crosses into the 1864th turn well
        // before the Dokkaebi King's monetisation of subway 3807. She
        // lives the lead-up years as the web-novel author "tls123" and
        // serialises *Three Ways to Survive in a Ruined World* — the
        // 3,149-chapter text Kim Dokja has been reading for a decade by
        // the time Bihyung opens channel #BI-7623. This is why TWSA
        // exists in-universe: it is the 1863rd turn written down by the
        // same author, set into the 1864th turn by the clone before the
        // scenarios begin. The reveal that tls123 is Han Sooyoung lands
        // in Epilogue 4. Arc-anchored to Arc 1 since the arrival predates
        // ch 1.
        await UpsertJumpAsync(db,
            characterLabel: "Han Sooyoung (clone, becomes tls123)",
            sourceWorldlineId: prevLine.Id, sourceOrder: 95d,
            targetWorldlineId: mainLine.Id, targetOrder: 0d,
            description:
                "A clone of the 1863rd-turn Han Sooyoung crosses into the 1864th turn well " +
                "before the Dokkaebi King's monetisation of subway 3807. She lives the lead-up " +
                "years as the web-novel author *tls123* and serialises *Three Ways to Survive " +
                "in a Ruined World* — the 3,149-chapter account of (essentially) the 1863rd turn " +
                "that Kim Dokja reads for over a decade. This is the in-universe origin of TWSA " +
                "and the loop that makes Kim Dokja's *Omniscient Reader's Viewpoint* possible.",
            lengthEstimate: "years lived in 1864 prior to Arc 1",
            arcId: arc1.Id,
            ct: ct);

        // (2) Kim Dokja: 1864th → 1863rd. Episode 84 (chapters 445–448) is
        // titled "1864" — Kim Dokja uses *Omniscient Reader's Viewpoint* to
        // step into the 1863rd-turn Yoo Joonghyuk's recorded life. It's a
        // soul-form pilgrimage, not a body relocation, but in the Jump model
        // a soul moving across worldlines is exactly what we want to draw.
        await UpsertJumpAsync(db,
            characterLabel: "Kim Dokja",
            sourceWorldlineId: mainLine.Id, sourceOrder: 75d,
            targetWorldlineId: prevLine.Id, targetOrder: 95d,
            description:
                "During Ep. 84 (*1864*) Kim Dokja activates *Omniscient Reader's Viewpoint* on " +
                "Yoo Joonghyuk's full 1863-turn history to understand the protagonist whose story " +
                "he has been reading for a decade. He sees the lives that produced the Secretive " +
                "Plotter — including the 3rd-turn Yoo Joonghyuk who escaped being a [Character].",
            lengthEstimate: "subjective: 1863 lifetimes · objective: minutes",
            ct: ct);

        // (3) Han Sooyoung: 1864th → 1865th. The rescue jump that opens the
        // 1865th turn. She becomes the *Architect of the False Last Act* and
        // burns her constellation status to send the surviving companions
        // (and a child form of Kim Dokja) one turn forward. First named in
        // ch 452, landing in ch 540.
        await UpsertJumpAsync(db,
            characterLabel: "Han Sooyoung",
            sourceWorldlineId: mainLine.Id, sourceOrder: 92d,
            targetWorldlineId: nextLine.Id, targetOrder: 5d,
            description:
                "Han Sooyoung's *Eye of Truth* finally penetrates [The Fourth Wall] in the 1864th " +
                "turn's collapse. She rewrites the ending: rather than vanish with Kim Dokja, the " +
                "carriage-3807 survivors plus a regressed-to-childhood Kim Dokja wake in Seoul " +
                "Gwanghwamun of a never-before-written 1865th turn.",
            lengthEstimate: "instantaneous (Architect of the False Last Act)",
            ct: ct);

        // (4) Kim Dokja: 1864th → 0th. Epilogue 1, *World of Zero*. After
        // splitting an Avatar off to live on with the companions, the rest
        // of Kim Dokja walks the regression backwards to its origin so he
        // can witness the very first Yoo Joonghyuk — the one who never had
        // a sponsor — without changing him.
        await UpsertJumpAsync(db,
            characterLabel: "Kim Dokja",
            sourceWorldlineId: mainLine.Id, sourceOrder: 95d,
            targetWorldlineId: zerothLine.Id, targetOrder: 50d,
            description:
                "Kim Dokja splits a 49% Avatar to live with the companions, then walks his " +
                "remaining self past every prior regression turn to the 0th turn — the *World " +
                "of Zero* of Epilogue 1. He watches the original Yoo Joonghyuk's first and only " +
                "life from outside the loop, narrated by [The Fourth Wall].",
            lengthEstimate: "subjective: ~1864 turns · objective: post-scenarios",
            ct: ct);

        // Note: an earlier draft of this seeder had a fifth jump
        //   (Han Sooyoung 1865th → 0th, "writes Ways of Survival from the
        //   First World-line") — that was wrong. The novel-in-novel TWSA is
        // authored by the 1863rd-turn Han Sooyoung clone landing in the early
        // 1864th turn (jump 1b above), not by an Epilogue-3 jump to the 0th
        // turn. Epilogue 3's possession scene is a within-worldline phenomenon
        // (her ego sharing a body across a day/night cycle), not a worldline
        // crossing, so it isn't a Jump in this model.
    }
}
