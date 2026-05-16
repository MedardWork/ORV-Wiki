'use strict';

/* ============================================================
   VIEW: HOME
============================================================ */
function renderHome() {
  const app = $('#app');
  app.innerHTML = '';
  const v = el('div', { class:'view' });

  v.appendChild(el('section', { class:'hero' },
    el('div', { class:'sub' }, 'A spoiler-aware archive of'),
    el('h1', {}, 'Omniscient Reader'),
    el('div', { class:'sub' }, 'Three Ways to Survive the Star Stream'),
    el('div', { class:'quote' },
      '"This is the story of only me — the sole reader who knew how the world would end."',
      el('span', { class:'quote-author' }, '— Kim Dokja'),
    ),
    el('div', { class:'hero-cta' },
      el('button', { class:'btn btn-primary', onclick:() => navigate('category/character') }, '✦ Browse Characters'),
      el('button', { class:'btn btn-secondary', onclick:() => navigate('timeline') }, '∞ View Timeline'),
      State.user
        ? el('button', { class:'btn btn-secondary', onclick:openChapterModal }, '📖 Set reading chapter')
        : el('button', { class:'btn btn-secondary', onclick:() => openAuth('register') }, 'Join the Archive'),
    ),
  ));

  // Categories
  v.appendChild(el('div', { class:'section-title' }, el('h2', {}, '✦ Domains of Knowledge'), el('div', { class:'ornament' })));
  const grid = el('div', { class:'cat-grid' });
  for (const [key, info] of Object.entries(ENTITY_TYPES)) {
    grid.appendChild(el('div', { class:'cat-tile', onclick:() => navigate('category/'+key) },
      el('span', { class:'glyph' }, info.glyph),
      el('h4', {}, info.plural),
      el('p', {}, info.desc),
    ));
  }
  v.appendChild(grid);

  // Recently visible (for logged-in)
  if (State.user) {
    const rec = el('div', { id:'recent-pages' });
    v.appendChild(el('div', { class:'section-title' }, el('h2', {}, '★ Recently Revealed'), el('div', { class:'ornament' })));
    v.appendChild(rec);
    api.get('/api/pages?page=1&pageSize=8').then(r => {
      rec.innerHTML = '';
      const items = r?.items || r?.Items || [];
      if (items.length === 0) {
        rec.appendChild(el('div', { class:'empty', style:{ padding:'1.5rem' }},
          el('p', {}, 'No pages visible yet — set your reading chapter to begin revealing the archive.')));
        return;
      }
      const g = el('div', { class:'page-grid' });
      items.forEach(p => g.appendChild(pageCard(p)));
      rec.appendChild(g);
    }).catch(e => { rec.innerHTML = ''; rec.appendChild(errorBlock(e)); });
  } else {
    v.appendChild(el('div', { class:'side-card', style:{ marginTop:'2rem', textAlign:'center', padding:'1.5rem' }},
      el('h3', {}, '⚠ The Archive is Spoiler-Aware'),
      el('p', { style:{ color:'var(--text-2)', maxWidth:'560px', margin:'.6rem auto' }},
        'Sign in to set your current reading chapter. Pages introduced later, and inline spoilers, will remain hidden until you reach them.'),
      el('button', { class:'btn btn-primary', onclick:() => openAuth('register') }, 'Begin Reading'),
    ));
  }

  app.appendChild(v);
}

/* ============================================================
   VIEW: CATEGORY LIST
============================================================ */
function renderCategory(type, page=1) {
  if (!ENTITY_TYPES[type]) {
    $('#app').innerHTML = '<div class="empty"><h3>Unknown category</h3></div>';
    return;
  }
  const info = ENTITY_TYPES[type];
  page = parseInt(page) || 1;
  const app = $('#app');
  app.innerHTML = '';
  const v = el('div', { class:'view' });
  v.appendChild(crumbs([['', 'Archive'], [null, info.plural]]));
  v.appendChild(el('div', { class:'detail-header' },
    el('div', { class:'type' }, info.glyph + ' ' + info.single + ' Category'),
    el('h1', {}, info.plural),
    el('div', { class:'summary' }, info.desc + '. Entries appear here only if your current reading chapter has reached their introduction.'),
  ));

  const listEl = el('div', { class:'page-grid' }, el('div', { class:'loader' }, el('div', { class:'loader-spinner' })));
  const pagEl  = el('div', {});
  v.appendChild(listEl);
  v.appendChild(pagEl);
  app.appendChild(v);

  api.get(`/api/pages?page=${page}&pageSize=24&entityType=${type}`).then(r => {
    listEl.innerHTML = '';
    const items = r?.items || r?.Items || [];
    const total = r?.total ?? r?.Total ?? items.length;
    const ps    = r?.pageSize ?? r?.PageSize ?? 24;
    if (items.length === 0) {
      listEl.replaceWith(el('div', { class:'empty' },
        el('div', { class:'glyph' }, info.glyph),
        el('h3', {}, 'No ' + info.plural + ' visible yet'),
        el('p', {}, State.user ? 'Advance your reading to reveal more entries.' : 'Sign in and set your reading chapter to begin.'),
      ));
      return;
    }
    items.forEach(p => listEl.appendChild(pageCard(p)));
    pagEl.replaceWith(paginator(page, ps, total, n => navigate(`category/${type}/${n}`)));
  }).catch(e => { listEl.innerHTML = ''; listEl.appendChild(errorBlock(e)); });
}

function pageCard(p) {
  const type = p.entityType || p.entity_type;
  const info = ENTITY_TYPES[type] || { glyph:'?', single:'Page' };
  const route = type === 'character' ? 'character/'+p.slug : 'page/'+p.slug;
  // Title and ShortDescription are RenderedContent. We use renderRendered
  // for the description (so hidden-spoiler chips appear inline) and let the
  // title go through the same renderer too — page titles rarely contain
  // [spoiler] markup but the renderer handles plain strings as a no-op.
  const titleNode = el('h3', {}); renderRendered(p.title, titleNode);
  const descBox = el('div', { class:'desc' });
  const summary = p.shortDescription || p.short_description;
  if (hasContent(summary)) {
    renderRendered(summary, descBox);
  } else {
    descBox.appendChild(el('span', { style:{ color:'var(--text-3)', fontStyle:'italic' }}, 'No summary yet.'));
  }
  return el('div', { class:'page-card', onclick:() => navigate(route) },
    el('div', { class:'type' }, info.glyph + '  ' + info.single),
    titleNode,
    descBox,
    el('div', { class:'footer' },
      el('span', { class:'ch' }, 'Ch. ' + (p.discoveryChapter ?? p.discovery_chapter ?? '?')),
      el('span', {}, '↗'),
    ),
  );
}

function paginator(page, pageSize, total, onJump) {
  const last = Math.max(1, Math.ceil(total / pageSize));
  const wrap = el('div', { class:'pagination' });
  wrap.appendChild(el('button', { class:'btn btn-secondary btn-sm', disabled: page<=1, onclick:() => onJump(page-1) }, '← Prev'));
  wrap.appendChild(el('span', { class:'info' }, `Page ${page} of ${last} · ${total} entries`));
  wrap.appendChild(el('button', { class:'btn btn-secondary btn-sm', disabled: page>=last, onclick:() => onJump(page+1) }, 'Next →'));
  return wrap;
}

function crumbs(items) {
  const c = el('div', { class:'crumbs' });
  items.forEach((pair, i) => {
    const [route, label] = pair;
    if (i > 0) c.appendChild(el('span', { class:'sep' }, '›'));
    if (route !== null) c.appendChild(el('a', { href:'#/'+route }, label));
    else c.appendChild(el('span', {}, label));
  });
  return c;
}

function errorBlock(err) {
  return el('div', { class:'empty' },
    el('div', { class:'glyph' }, '⚠'),
    el('h3', {}, err.status ? 'Error '+err.status : 'Connection lost'),
    el('p', {}, err.message),
    err.status === 401 ? el('button', { class:'btn btn-primary', style:{ marginTop:'.6rem' }, onclick:() => openAuth('login') }, 'Sign in') : null,
    !err.status ? el('button', { class:'btn btn-secondary', style:{ marginTop:'.6rem' }, onclick:openSettings }, 'Open settings') : null,
  );
}

/* Spoiler-rendering helpers (plainTextOf, hasContent, hasHidden,
   renderRendered, renderedNode, spoilerBanner) live in js/spoiler.js. */

/* ============================================================
   VIEW: GENERIC PAGE DETAIL

   PageDto returns Title and ShortDescription as RenderedContent.
   For non-character entity types we then *also* fetch the
   entity-specific endpoint (e.g. /api/constellations/by-slug/{slug})
   to surface narrative fields (Description, Effect, Conditions, …)
   that PageDto alone does not carry. The list of fields per type
   lives in ENTITY_TYPES[type].narratives.
============================================================ */
async function renderPage(slug) {
  const app = $('#app');
  app.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
  try {
    const p = await api.get('/api/pages/' + encodeURIComponent(slug));
    const type = p.entityType || p.entity_type;
    const info = ENTITY_TYPES[type] || { glyph:'?', single:'Page' };
    if (type === 'character') return renderCharacter(slug);
    app.innerHTML = '';
    const v = el('div', { class:'view' });
    v.appendChild(crumbs([
      ['', 'Archive'],
      ['category/'+type, info.plural],
      [null, plainTextOf(p.title) || p.slug],
    ]));

    const layout = el('div', { class:'detail-layout' });
    const main = el('div', {});
    main.appendChild(detailHeader(p, info));

    // Short description (from PageDto) — RenderedContent.
    if (hasContent(p.shortDescription || p.short_description)) {
      const body = el('div', { class:'detail-body' });
      body.appendChild(el('h2', {}, 'Overview'));
      body.appendChild(renderedNode(p.shortDescription || p.short_description, 'p'));
      if (hasHidden(p.shortDescription || p.short_description)) body.appendChild(spoilerBanner());
      main.appendChild(body);
    }

    // Entity-specific narrative fields. We fire this in parallel with the
    // header render so the user sees the page header immediately while
    // the richer text loads.
    if (info.endpoint && info.narratives && info.narratives.length) {
      const placeholder = el('div', { class:'detail-body' },
        el('div', { class:'loader', style:{ minHeight:'80px' }}, el('div', { class:'loader-spinner' })));
      main.appendChild(placeholder);
      api.get(info.endpoint + '/by-slug/' + encodeURIComponent(slug))
        .then(entity => {
          const narrativeBlock = el('div', {});
          let any = false;
          for (const [key, label] of info.narratives) {
            const v = entity[key] ?? entity[key.charAt(0).toUpperCase()+key.slice(1)];
            if (!hasContent(v)) continue;
            any = true;
            const sec = el('div', { class:'detail-body' });
            sec.appendChild(el('h2', {}, label));
            sec.appendChild(renderedNode(v, 'p'));
            if (hasHidden(v)) sec.appendChild(spoilerBanner());
            narrativeBlock.appendChild(sec);
          }
          // Structural facts that aren't RenderedContent — show as a small profile block.
          const profile = entityProfile(type, entity);
          if (profile) narrativeBlock.appendChild(profile);
          // Relationship sections (e.g. a scenario's locations, a location's scenarios).
          const rels = entityRelationSections(type, entity);
          for (const sec of rels) narrativeBlock.appendChild(sec);
          placeholder.replaceWith(any || profile || rels.length ? narrativeBlock : el('div', {}));
        })
        .catch(() => {
          // Entity endpoint may 404 if the spoiler gate hides the entity even
          // when the page row itself is somehow visible. Fail soft — the page
          // header is already on screen.
          placeholder.remove();
        });
    }

    main.appendChild(commentsSection(p));

    layout.appendChild(main);
    layout.appendChild(sidePanel(p));
    v.appendChild(layout);
    app.appendChild(v);
  } catch (e) { app.innerHTML = ''; app.appendChild(errorBlock(e)); }
}

// Per-entity-type structural facts (not RenderedContent) shown as a profile.
// Returns null when there's nothing useful to display.
function entityProfile(type, e) {
  const rows = [];
  const push = (k, v) => { if (v != null && v !== '') rows.push([k, v]); };
  switch (type) {
    case 'constellation':
      push('Modifier',  e.modifier ?? e.Modifier);
      push('True name', e.trueName ?? e.TrueName);
      push('Grade',     e.grade ?? e.Grade);
      break;
    case 'dokkaebi':
      push('Channel', e.channelId ?? e.channel_id ?? e.ChannelId);
      push('Rank',    e.rank ?? e.Rank);
      break;
    case 'item':
      push('Grade',         e.itemGrade ?? e.item_grade ?? e.Grade);
      push('Star relic',    (e.isStarRelic ?? e.is_star_relic ?? e.IsStarRelic) ? 'Yes' : null);
      break;
    case 'skill':
      push('Type',          e.skillType ?? e.skill_type ?? e.SkillType);
      push('Cost (coins)',  e.costInCoins ?? e.cost_in_coins ?? e.CostInCoins);
      push('Max level',     e.maxLevel ?? e.max_level ?? e.MaxLevel);
      break;
    case 'attribute':
      push('Rarity', e.rarity ?? e.Rarity);
      break;
    case 'stigma':
      push('Activation cost', e.activationCost ?? e.activation_cost ?? e.ActivationCost);
      break;
    case 'scenario':
      push('Code',       e.code ?? e.Code);
      push('Type',       e.type ?? e.Type);
      push('Difficulty', e.difficulty ?? e.Difficulty);
      push('Chapters',   ((e.chapterStart ?? e.chapter_start) ?? '?') + '–' + ((e.chapterEnd ?? e.chapter_end) ?? '?'));
      break;
    case 'arc':
      push('Order',    e.orderNumber ?? e.order_number ?? e.OrderNumber);
      push('Chapters', ((e.chapterStart ?? e.chapter_start) ?? '?') + '–' + ((e.chapterEnd ?? e.chapter_end) ?? '?'));
      break;
    case 'location':
      push('Dimension', e.dimension ?? e.Dimension);
      break;
    case 'worldline':
      push('Line number', e.lineNumber ?? e.line_number ?? e.LineNumber);
      push('Main',        (e.isMain ?? e.is_main ?? e.IsMain) ? 'Yes' : null);
      break;
    case 'event':
      push('Chapter',    e.chapterNumber ?? e.chapter_number ?? e.ChapterNumber);
      push('Importance', e.importance ?? e.Importance);
      break;
    case 'concept':
      push('Impact', e.impactLevel ?? e.impact_level ?? e.ImpactLevel);
      break;
  }
  if (!rows.length) return null;
  const card = el('div', { class:'detail-body' });
  card.appendChild(el('h2', {}, 'Profile'));
  for (const [k, v] of rows) {
    card.appendChild(el('div', { class:'field-row' },
      el('div', { class:'field-label' }, k),
      el('div', { class:'field-value' }, String(v)),
    ));
  }
  return card;
}

function detailHeader(p, info) {
  const titleNode = el('h1', {}); renderRendered(p.title, titleNode);
  const summary = p.shortDescription || p.short_description;
  return el('div', { class:'detail-header' },
    el('div', { class:'type' }, info.glyph + '  ' + info.single),
    titleNode,
    hasContent(summary) ? renderedNode(summary, 'div', { class:'summary' }) : null,
    el('div', { class:'meta-row' },
      el('span', {}, '📖 First seen: ', el('strong', {}, 'Ch. '+ (p.discoveryChapter ?? p.discovery_chapter))),
      el('span', {}, '👁 ', el('strong', {}, (p.viewCount ?? p.view_count ?? 0).toLocaleString()), ' views'),
      el('span', {}, '🜲 ', el('strong', {}, '/' + p.slug)),
    ),
    tagRow(p.tags),
  );
}

function sidePanel(p) {
  const sb = el('div', { class:'sidebar' });
  if (State.user) {
    const bookmarkBtn = el('button', { class:'btn btn-secondary bookmark-btn', style:{ width:'100%' }, onclick:async () => {
      try {
        const r = await api.post('/api/bookmarks/toggle/'+p.id);
        const on = r?.bookmarked ?? r?.Bookmarked;
        bookmarkBtn.classList.toggle('active', !!on);
        bookmarkBtn.lastChild.textContent = on ? 'Bookmarked' : 'Bookmark this page';
        toast(on ? 'Bookmarked' : 'Bookmark removed');
      } catch (e) { toast(e.message, 'error'); }
    }}, el('span', {}, 'Bookmark this page'));
    sb.appendChild(el('div', { class:'side-card' },
      el('h4', {}, 'Reader Tools'),
      bookmarkBtn,
      el('button', { class:'btn btn-ghost btn-sm', style:{ width:'100%', marginTop:'.4rem' }, onclick:() => openSuggest(p) }, '✎ Suggest an edit'),
    ));
  }

  sb.appendChild(el('div', { class:'side-card' },
    el('h4', {}, 'Page Metadata'),
    el('div', { class:'row' }, el('span', { class:'k' }, 'Entity type'), el('span', { class:'v' }, p.entityType || p.entity_type || '?')),
    el('div', { class:'row' }, el('span', { class:'k' }, 'Slug'), el('span', { class:'v' }, p.slug)),
    el('div', { class:'row' }, el('span', { class:'k' }, 'Discovery'), el('span', { class:'v' }, 'Ch. '+(p.discoveryChapter ?? p.discovery_chapter))),
    el('div', { class:'row' }, el('span', { class:'k' }, 'Views'), el('span', { class:'v' }, (p.viewCount ?? p.view_count ?? 0).toLocaleString())),
    el('div', { class:'row' }, el('span', { class:'k' }, 'Updated'), el('span', { class:'v' }, fmtDate(p.updatedAt || p.updated_at))),
  ));

  sb.appendChild(el('div', { class:'side-card' },
    el('h4', {}, 'About the Archive'),
    el('p', { style:{ color:'var(--text-2)', fontSize:'.85rem' }},
      'Each page only becomes visible after you reach its discovery chapter. Inline ',
      el('code', {}, '[spoiler]'), ' segments are gated server-side — hidden text never leaves the database.'),
  ));
  return sb;
}

/* ============================================================
   VIEW: CHARACTER DETAIL
   CharacterDto carries Title and ShortDescription as RenderedContent
   (page-derived) plus its own narrative field Biography. The structural
   columns (FullName, Alias, Species, Status, Gender, BirthChapter,
   DeathChapter) are passthrough and are shown as a small profile card.
============================================================ */
async function renderCharacter(slug) {
  const app = $('#app');
  app.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
  try {
    const c = await api.get('/api/characters/by-slug/' + encodeURIComponent(slug));
    const info = ENTITY_TYPES.character;
    const fullName = c.fullName || c.full_name;
    const discoveryCh = c.discoveryChapter ?? c.discovery_chapter;
    const pageId  = c.pageId ?? c.page_id;
    // Synthesize a Page-shaped object so comments + sidePanel keep working
    // without needing the entity to also include a nested .page.
    const pageData = {
      id: pageId, slug, title: fullName, entityType:'character',
      shortDescription: c.shortDescription ?? c.short_description,
      discoveryChapter: discoveryCh,
    };
    app.innerHTML = '';
    const v = el('div', { class:'view' });
    v.appendChild(crumbs([['', 'Archive'], ['category/character', 'Characters'], [null, fullName]]));

    const layout = el('div', { class:'detail-layout' });
    const main = el('div', {});

    const summary = c.shortDescription ?? c.short_description;
    main.appendChild(el('div', { class:'detail-header' },
      el('div', { class:'type' }, info.glyph + '  Character' + (c.alias ? ' · ' + c.alias : '')),
      el('h1', {}, fullName),
      hasContent(summary)
        ? renderedNode(summary, 'div', { class:'summary' })
        : el('div', { class:'summary' }, c.species ? `${cap(c.species)} · ${cap(c.status||c.Status||'unknown')}` : cap(c.status||c.Status||'unknown')),
      el('div', { class:'meta-row' },
        (c.birthChapter ?? c.birth_chapter) ? el('span', {}, '◐ Born: ', el('strong', {}, 'Ch. '+(c.birthChapter ?? c.birth_chapter))) : null,
        (c.deathChapter ?? c.death_chapter) ? el('span', {}, '☠ Died: ', el('strong', {}, 'Ch. '+(c.deathChapter ?? c.death_chapter))) : null,
        c.gender ? el('span', {}, '⚥ ', el('strong', {}, cap(c.gender))) : null,
        discoveryCh != null ? el('span', {}, '📖 First seen: ', el('strong', {}, 'Ch. '+discoveryCh)) : null,
      ),
      tagRow(c.tags),
    ));

    // Biography (RenderedContent — may carry inline spoilers).
    const bio = c.biography ?? c.Biography;
    if (hasContent(bio)) {
      const body = el('div', { class:'detail-body' });
      body.appendChild(el('h2', {}, 'Biography'));
      body.appendChild(renderedNode(bio, 'p'));
      if (hasHidden(bio)) body.appendChild(spoilerBanner(
        '✦ This biography contains revelations beyond your current reading chapter. Continue reading to unlock them.'));
      main.appendChild(body);
    }

    // Profile (structural fields)
    const fields = [
      ['Alias', c.alias],
      ['Species', c.species && cap(c.species)],
      ['Status', c.status && cap(c.status)],
      ['Gender', c.gender && cap(c.gender)],
    ].filter(f => f[1]);
    if (fields.length) {
      const body = el('div', { class:'detail-body' });
      body.appendChild(el('h2', {}, 'Profile'));
      for (const [k, val] of fields) {
        body.appendChild(el('div', { class:'field-row' },
          el('div', { class:'field-label' }, k),
          el('div', { class:'field-value' }, val),
        ));
      }
      main.appendChild(body);
    }

    // Relationships — navigable links to every related entity the character
    // detail payload surfaces (already spoiler-filtered by the API).
    for (const section of characterRelationSections(c)) main.appendChild(section);

    if (pageId) main.appendChild(commentsSection(pageData));

    layout.appendChild(main);
    layout.appendChild(sidePanel(pageData));
    v.appendChild(layout);
    app.appendChild(v);
  } catch (e) { app.innerHTML = ''; app.appendChild(errorBlock(e)); }
}
function cap(s) { return s ? String(s)[0].toUpperCase()+String(s).slice(1) : ''; }
function humanize(s) { return s == null || s === '' ? '' : cap(String(s).replace(/_/g, ' ')); }

/* ============================================================
   CHARACTER RELATIONSHIP SECTIONS
   CharacterDetailDto carries ten relationship arrays. Each entry
   becomes a clickable chip routing to the related entity's page
   (page/{slug}). relSection returns null for an empty array so the
   section is skipped entirely.
============================================================ */
function relSection(label, glyph, items, map) {
  if (!items || !items.length) return null;
  const grid = el('div', { class:'rel-grid' });
  for (const it of items) {
    const [slug, name, meta] = map(it);
    grid.appendChild(el('div', {
        class:'rel-chip', title:'Open ' + name,
        onclick:() => navigate('page/' + slug) },
      el('span', { class:'rel-name' }, name),
      meta ? el('span', { class:'rel-meta' }, meta) : null,
    ));
  }
  return el('div', { class:'detail-body' }, el('h2', {}, glyph + '  ' + label), grid);
}

function characterRelationSections(c) {
  const E = ENTITY_TYPES;
  const ch = n => n != null ? 'Ch.' + n : null;
  const join = parts => parts.filter(Boolean).join(' · ');
  return [
    relSection('Skills', E.skill.glyph, c.skills, s =>
      [s.slug, s.name, join([humanize(s.skillType), 'Lv.' + s.level, ch(s.acquiredChapter)])]),
    relSection('Stigmas', E.stigma.glyph, c.stigmas, s =>
      [s.slug, s.name, join([s.isPrimary ? 'Primary' : null, ch(s.acquiredChapter)])]),
    relSection('Attributes', E.attribute.glyph, c.attributes, a =>
      [a.slug, a.name, join([humanize(a.rarity), ch(a.acquiredChapter)])]),
    relSection('Items', E.item.glyph, c.items, i =>
      [i.slug, i.name, join([humanize(i.grade), ch(i.acquiredChapter),
        i.lostChapter != null ? 'lost Ch.' + i.lostChapter : null])]),
    relSection('Fables', E.fable.glyph, c.fables, f =>
      [f.slug, f.title, join([humanize(f.grade), ch(f.acquiredChapter)])]),
    relSection('Sponsoring Constellations', E.constellation.glyph, c.constellations, k =>
      [k.slug, k.modifier, join([humanize(k.relationshipType), ch(k.sinceChapter)])]),
    relSection('Ascended Constellations', E.constellation.glyph, c.deifiedConstellations, k =>
      [k.slug, k.modifier, humanize(k.grade)]),
    relSection('Originated Fables', E.fable.glyph, c.originatedFables, f =>
      [f.slug, f.title, humanize(f.grade)]),
    relSection('Events', E.event.glyph, c.events, e =>
      [e.slug, e.title, join(['Ch.' + e.chapterNumber, humanize(e.role)])]),
    relSection('Scenarios', E.scenario.glyph, c.scenarios, s =>
      [s.slug, s.title, join([humanize(s.type), humanize(s.outcome)])]),
  ].filter(Boolean);
}

// Relationship sections for the generic page view — currently the
// scenario↔location links carried by ScenarioDto / LocationDto.
function entityRelationSections(type, e) {
  const E = ENTITY_TYPES;
  if (type === 'scenario')
    return [relSection('Locations', E.location.glyph, e.locations,
      l => [l.slug, l.name, null])].filter(Boolean);
  if (type === 'location')
    return [relSection('Scenarios', E.scenario.glyph, e.scenarios,
      s => [s.slug, s.title, humanize(s.type)])].filter(Boolean);
  return [];
}

/* ============================================================
   TAG CHIPS — clickable labels linking to a tag's filtered page list
============================================================ */
function tagRow(tags) {
  if (!tags || !tags.length) return null;
  const row = el('div', { class:'tag-row' });
  for (const t of tags) {
    row.appendChild(el('span', {
        class:'tag-link', title:'Browse pages tagged ' + t.name,
        onclick:() => navigate('tag/' + t.slug) },
      t.color ? el('span', { class:'tag-dot', style:{ background:t.color }}) : null,
      t.name,
    ));
  }
  return row;
}

/* ============================================================
   VIEW: TAG — pages carrying a given tag (spoiler-gated)
============================================================ */
async function renderTag(slug, page=1) {
  page = parseInt(page) || 1;
  const app = $('#app');
  app.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
  try {
    const [res, tags] = await Promise.all([
      api.get(`/api/pages?page=${page}&pageSize=24&tag=${encodeURIComponent(slug)}`),
      api.get('/api/tags').catch(() => []),
    ]);
    const tag = (tags || []).find(t => t.slug === slug);
    const items = res?.items || res?.Items || [];
    const total = res?.total ?? res?.Total ?? items.length;
    const ps    = res?.pageSize ?? res?.PageSize ?? 24;
    app.innerHTML = '';
    const v = el('div', { class:'view' });
    v.appendChild(crumbs([['', 'Archive'], [null, tag?.name || slug]]));
    v.appendChild(el('div', { class:'detail-header' },
      el('div', { class:'type' }, '⌗ Tag'),
      el('h1', {}, tag?.name || slug),
      el('div', { class:'summary' }, 'Pages carrying this tag that your current reading chapter has revealed.'),
    ));
    if (!items.length) {
      v.appendChild(el('div', { class:'empty' },
        el('div', { class:'glyph' }, '⌗'),
        el('h3', {}, 'No tagged pages visible yet'),
        el('p', {}, State.user ? 'Advance your reading to reveal more entries.' : 'Sign in and set your reading chapter to begin.'),
      ));
    } else {
      const grid = el('div', { class:'page-grid' });
      items.forEach(p => grid.appendChild(pageCard(p)));
      v.appendChild(grid);
      v.appendChild(paginator(page, ps, total, n => navigate(`tag/${slug}/${n}`)));
    }
    app.appendChild(v);
  } catch (e) { app.innerHTML = ''; app.appendChild(errorBlock(e)); }
}
