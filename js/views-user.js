'use strict';

/* ============================================================
   VIEW: BOOKMARKS
============================================================ */
async function renderBookmarks() {
  if (!State.user) return openAuth('login'), navigate('');
  const app = $('#app');
  app.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
  try {
    const r = await api.get('/api/bookmarks?page=1&pageSize=50');
    const items = r?.items || r || [];
    app.innerHTML = '';
    const v = el('div', { class:'view' });
    v.appendChild(crumbs([['', 'Archive'], [null, 'My Bookmarks']]));
    v.appendChild(el('div', { class:'detail-header' },
      el('div', { class:'type' }, '★ Bookmarks'),
      el('h1', {}, 'Saved Pages'),
      el('div', { class:'summary' }, 'Pages you have bookmarked across the archive.'),
    ));
    if (items.length === 0) {
      v.appendChild(el('div', { class:'empty' }, el('div', { class:'glyph' }, '☆'), el('h3', {}, 'No bookmarks yet'), el('p', {}, 'Bookmark a page from its sidebar.')));
    } else {
      const grid = el('div', { class:'page-grid' });
      items.forEach(b => {
        // Bookmark may carry { page: {...} } or flat fields
        const page = b.page || b.Page || { slug: b.slug, title: b.title, entityType: b.entityType, shortDescription: b.shortDescription, discoveryChapter: b.discoveryChapter };
        if (!page.slug) return;
        grid.appendChild(pageCard(page));
      });
      v.appendChild(grid);
    }
    app.appendChild(v);
  } catch (e) { app.innerHTML = ''; app.appendChild(errorBlock(e)); }
}

/* ============================================================
   VIEW: MY SUGGESTIONS
============================================================ */
async function renderMySuggestions() {
  if (!State.user) return openAuth('login'), navigate('');
  const app = $('#app');
  app.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
  try {
    const r = await api.get('/api/edit-suggestions/mine?page=1&pageSize=30');
    const items = r?.items || r || [];
    app.innerHTML = '';
    const v = el('div', { class:'view' });
    v.appendChild(crumbs([['', 'Archive'], [null, 'My Edit Suggestions']]));
    v.appendChild(el('div', { class:'detail-header' },
      el('div', { class:'type' }, '✎ Edit Suggestions'),
      el('h1', {}, 'My Proposals'),
      el('div', { class:'summary' }, 'Edits you have proposed and their review status.'),
    ));
    v.appendChild(suggestionsList(items));
    app.appendChild(v);
  } catch (e) { app.innerHTML = ''; app.appendChild(errorBlock(e)); }
}

async function renderQueue(status) {
  if (!State.user) return openAuth('login'), navigate('');
  status = status || 'pending';
  const app = $('#app');
  app.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
  try {
    const statusQuery = status === 'all' ? '' : '&status=' + status;
    const r = await api.get('/api/edit-suggestions?page=1&pageSize=40' + statusQuery);
    const items = r?.items || r || [];
    app.innerHTML = '';
    const v = el('div', { class:'view' });
    v.appendChild(crumbs([['', 'Archive'], [null, 'Suggestion Queue']]));
    v.appendChild(el('div', { class:'detail-header' },
      el('div', { class:'type' }, '⚖ Editor Review'),
      el('h1', {}, 'Edit Suggestions'),
      el('div', { class:'summary' }, 'Approve or reject reader-submitted edits. Editor-applied changes also appear here as change history.'),
    ));
    const filter = el('select', { onchange:e => renderQueue(e.target.value) },
      el('option', { value:'pending' }, 'Pending review'),
      el('option', { value:'approved' }, 'Approved'),
      el('option', { value:'rejected' }, 'Rejected'),
      el('option', { value:'all' }, 'All'),
    );
    filter.value = status;
    v.appendChild(el('div', { class:'field', style:{ maxWidth:'240px', marginBottom:'1rem' }},
      el('label', {}, 'Show'), filter));
    v.appendChild(suggestionsList(items, true));
    app.appendChild(v);
  } catch (e) { app.innerHTML = ''; app.appendChild(errorBlock(e)); }
}

function suggestionsList(items, asReviewer=false) {
  if (!items.length) return el('div', { class:'empty' }, el('div', { class:'glyph' }, '✎'),
    el('h3', {}, 'Nothing to show'),
    el('p', {}, asReviewer ? 'No suggestions match this filter.' : 'You have not submitted any edits yet.'));
  const list = el('div', { style:{ display:'flex', flexDirection:'column', gap:'.6rem' }});
  const opColor = { create:'#6ed89a', update:'var(--gold-bright)', delete:'#e08585' };
  items.forEach(s => {
    const status = (s.status || 'pending').toLowerCase();
    const operation = (s.operation || 'update').toLowerCase();
    const entityType = s.entityType || s.entity_type;
    const typeInfo = ENTITY_TYPES[entityType] || { single: humanize(entityType) || 'Page', glyph:'◇' };
    const isMine = State.user && (s.userId ?? s.user_id) === State.user.id;
    const changes = s.proposedChanges || s.proposed_changes || {};
    const slug = s.pageSlug || s.page_slug;
    const titleText = s.pageTitle || s.page_title
      || (changes.fields && changes.fields.title) || ('New ' + typeInfo.single);
    const titleNode = slug ? el('a', { href:'#/page/' + slug }, titleText) : titleText;

    const card = el('div', { class:'detail-body', style:{ padding:'1rem 1.2rem', margin:0 }},
      el('div', { style:{ display:'flex', justifyContent:'space-between', alignItems:'flex-start', flexWrap:'wrap', gap:'.6rem' }},
        el('div', {},
          el('div', { style:{ display:'flex', alignItems:'center', gap:'.5rem', flexWrap:'wrap' }},
            el('span', { style:{ fontSize:'.64rem', textTransform:'uppercase', letterSpacing:'.06em',
              fontWeight:'700', padding:'.14rem .5rem', borderRadius:'4px', border:'1px solid var(--border)',
              color: opColor[operation] || 'var(--text-2)' }}, operation),
            el('h4', { style:{ margin:0 }}, titleNode),
          ),
          el('div', { style:{ fontSize:'.78rem', color:'var(--text-3)', marginTop:'.25rem' }},
            typeInfo.glyph + ' ' + typeInfo.single + ' · submitted ' + fmtDate(s.createdAt || s.created_at)
            + (s.username ? ' by ' + s.username : '')),
        ),
        el('span', { class:'status-badge status-' + status }, status),
      ),
      s.reason ? el('p', { style:{ marginTop:'.6rem', fontStyle:'italic', color:'var(--text-2)' }}, '"' + s.reason + '"') : null,
      renderDiff(changes),
      buildSuggestionActions(s, status, asReviewer, isMine),
    );
    list.appendChild(card);
  });
  return list;
}

function buildSuggestionActions(s, status, asReviewer, isMine) {
  const canReview = asReviewer && status === 'pending';
  if (!canReview && !isMine) return null;
  const row = el('div', { style:{ marginTop:'.8rem', display:'flex', gap:'.4rem', flexWrap:'wrap' }});
  if (canReview) {
    row.appendChild(el('button', { class:'btn btn-primary btn-sm', onclick:async () => {
      try { await api.post('/api/edit-suggestions/' + s.id + '/approve'); toast('Approved', 'success'); renderQueue(); }
      catch (e) { toast(e.message, 'error'); }
    }}, '✓ Approve'));
    row.appendChild(el('button', { class:'btn btn-danger btn-sm', onclick:async () => {
      try { await api.post('/api/edit-suggestions/' + s.id + '/reject'); toast('Rejected'); renderQueue(); }
      catch (e) { toast(e.message, 'error'); }
    }}, '✕ Reject'));
  }
  if (isMine) {
    row.appendChild(el('button', { class:'btn btn-ghost btn-sm', onclick:async () => {
      if (!confirm('Delete this suggestion? This cannot be undone.')) return;
      try {
        await api.del('/api/edit-suggestions/' + s.id);
        toast('Deleted');
        asReviewer ? renderQueue() : renderMySuggestions();
      } catch (e) { toast(e.message, 'error'); }
    }}, '🗑 Delete'));
  }
  return row.children.length ? row : null;
}

/* ============================================================
   VIEW: TIMELINE
============================================================ */
async function renderTimeline() {
  const app = $('#app');
  app.innerHTML = '';
  // Tag the document so .timeline-active CSS can break the timeline view out
  // of the centred 1400px content column and give it the full viewport.
  document.body.classList.add('timeline-active');
  const v = el('div', { class:'view view-timeline' });

  // Compact titlebar — eyebrow + h1 + spoiler hint on a single row, so the
  // canvas below gets the lion's share of the viewport. No filter controls:
  // the timeline is the page, not a tool you tweak.
  v.appendChild(el('header', { class:'tl-titlebar' },
    el('div', { class:'tl-titlebar-left' },
      el('span', { class:'tl-eyebrow' }, '∞  Chronological Timeline'),
      el('h1', { class:'tl-h1' }, 'The Worldlines'),
    ),
  ));

  const canvasWrap = el('div', { class:'timeline-canvas', id:'tl-canvas' });

  // Spoiler gate — full-canvas overlay. The reader must click "I understand"
  // to reveal the timeline beneath. We render it absolutely positioned
  // inside the canvas wrapper so it covers the lanes/jumps but lets the
  // titlebar and legend stay visible — the warning is *about* the canvas,
  // not the page chrome. Dismissed per-mount (re-shows on every visit) so
  // returning readers always re-confirm before seeing late-novel reveals.
  const spoilerGate = el('div', { class:'tl-spoiler-gate' },
    el('div', { class:'tl-spoiler-gate-content' },
      el('div', { class:'tl-spoiler-icon' }, '⚠'),
      el('h2', { class:'tl-spoiler-title' }, 'SPOILER-RICH ZONE'),
      el('p', { class:'tl-spoiler-body' },
        'This timeline maps the full regression chain end-to-end. It includes ' +
        'reveals from the final arcs and epilogues. If you are still reading, ' +
        'turn back now.',
      ),
      el('button', {
        class:'btn tl-spoiler-confirm',
        onclick:() => spoilerGate.remove(),
      }, 'I understand · Reveal the timeline'),
    ),
  );
  const legend = el('footer', { class:'tl-legend' },
    el('span', { class:'tl-legend-item' }, el('span', { class:'tl-legend-swatch tl-jump-swatch' }), 'Worldline jump (color = source line)'),
    el('span', { class:'tl-legend-item' }, el('span', { style:{ display:'inline-block', width:10, height:10, borderRadius:'50%', background:'var(--gold-bright)', boxShadow:'0 0 6px var(--gold-glow)' }}), 'Pivotal event'),
    el('span', { class:'tl-legend-item' }, el('span', { style:{ display:'inline-block', width:8, height:8, borderRadius:'50%', background:'var(--gold)' }}), 'Major event'),
    el('span', { class:'tl-legend-item' }, el('span', { style:{ display:'inline-block', width:6, height:6, borderRadius:'50%', background:'var(--text-3)' }}), 'Minor event'),
  );
  v.appendChild(canvasWrap);
  v.appendChild(legend);
  app.appendChild(v);

  // Cleanup: drop the body class when the user navigates away from the
  // timeline view, so subsequent pages get the normal centred layout back.
  // The router calls innerHTML='' on #app before rendering anywhere else,
  // and a MutationObserver catches that swap.
  const obs = new MutationObserver(() => {
    if (!document.body.contains(v)) {
      document.body.classList.remove('timeline-active');
      obs.disconnect();
    }
  });
  obs.observe(app, { childList:true });

  canvasWrap.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
  try {
    const data = await api.get('/api/timeline');
    drawTimeline(canvasWrap, data);
    // Mount the gate AFTER drawTimeline (which calls innerHTML='' on host
    // and would otherwise wipe the gate). It sits on top of the SVG.
    canvasWrap.appendChild(spoilerGate);
  } catch (e) { canvasWrap.innerHTML = ''; canvasWrap.appendChild(errorBlock(e)); }
}

function drawTimeline(host, data) {
  const worldlines = data?.worldlines || data?.Worldlines || [];
  const events     = data?.events     || data?.Events     || [];
  const jumps      = data?.jumps      || data?.Jumps      || [];

  if (worldlines.length === 0 && events.length === 0 && jumps.length === 0) {
    host.innerHTML = '';
    host.appendChild(el('div', { class:'empty' }, el('div', { class:'glyph' }, '∞'), el('h3', {}, 'No timeline data yet'), el('p', {}, 'Once worldlines and jumps are seeded, this view comes alive.')));
    return;
  }

  // Normalize
  const wls = worldlines.map(w => ({
    id: w.id ?? w.Id,
    line: w.lineNumber ?? w.line_number ?? w.LineNumber ?? 0,
    name: w.name ?? w.Name ?? null,
    isMain: w.isMain ?? w.is_main ?? w.IsMain ?? false,
    color: w.color ?? w.Color ?? null,
    order: w.displayOrder ?? w.display_order ?? w.DisplayOrder ?? 0,
  }));
  wls.sort((a,b) => (a.order - b.order) || (a.line - b.line));

  const evs = events.map(e => ({
    id: e.id ?? e.Id,
    title: e.title ?? e.Title ?? '',
    chapter: e.chapterNumber ?? e.chapter_number ?? e.ChapterNumber ?? 1,
    worldlineId: e.worldlineId ?? e.worldline_id ?? e.WorldlineId,
    importance: (e.importance ?? e.Importance ?? 'minor').toLowerCase(),
    // EventOrder is a within-chapter tiebreaker (1, 2, …), NOT the X axis
    // coordinate. ChapterNumber is the timeline position. We mix them at a
    // small fractional weight so multiple events sharing the same chapter
    // splay out instead of stacking, while staying close enough to that
    // chapter's tick to still read as "ch N".
    order: e.eventOrder ?? e.event_order ?? e.EventOrder ?? null,
    lengthEstimate: e.lengthEstimate ?? e.length_estimate ?? e.LengthEstimate ?? null,
  }));
  const evX = e => (e.chapter ?? 1) + ((e.order ?? 1) - 1) * 0.15;
  const jps = jumps.map(j => ({
    id: j.id ?? j.Id,
    characterLabel: j.characterLabel ?? j.character_label ?? j.CharacterLabel ?? '',
    description: j.description ?? j.Description ?? null,
    lengthEstimate: j.lengthEstimate ?? j.length_estimate ?? j.LengthEstimate ?? null,
    srcWl: j.sourceWorldlineId ?? j.source_worldline_id ?? j.SourceWorldlineId,
    srcOrder: j.sourceOrder ?? j.source_order ?? j.SourceOrder ?? 0,
    tgtWl: j.targetWorldlineId ?? j.target_worldline_id ?? j.TargetWorldlineId,
    tgtOrder: j.targetOrder ?? j.target_order ?? j.TargetOrder ?? 0,
  }));

  // Minor events clutter the lanes; the timeline is curated for major/pivotal
  // anchors (and jumps). Minor events still exist as data for their own pages.
  const visibleEvs = evs.filter(e => e.importance !== 'minor');

  const ORPHAN_ID = '__orphan__';
  if (visibleEvs.some(e => !e.worldlineId)) {
    wls.push({ id: ORPHAN_ID, line: '?', name: 'Unassigned', isMain:false, color:'#3d4670', order:9999 });
  }

  // Shared X scale: chapter-axis. Events sit at their chapter position
  // (with a small EventOrder splay to separate same-chapter events).
  // Jumps already publish source/target orders in chapter units, so the
  // two slot into the same scale without conversion.
  const allOrders = [
    ...visibleEvs.map(e => evX(e)),
    ...jps.map(j => j.srcOrder),
    ...jps.map(j => j.tgtOrder),
  ];
  const minOrder = allOrders.length ? Math.min(0, ...allOrders) : 0;
  const maxOrder = allOrders.length ? Math.max(10, ...allOrders) : 10;
  const orderRange = Math.max(1, maxOrder - minOrder);

  // Horizontal-lane layout. The canvas fits exactly to the host viewport
  // — no horizontal scroll. With a curated timeline (a handful of jumps,
  // not every chapter) compressing the chapter axis to a single screen
  // gives a far more readable overview than a wide canvas you have to
  // pan around to see in full. Lane height stays generous so cross-lane
  // arcs still have vertical room to bow without clipping labels.
  const padTop = 50, padBot = 50, padLeft = 220, padRight = 80;
  const laneH  = 160;
  const hostW  = Math.max(800, host.clientWidth - 32);
  const innerW = Math.max(600, hostW - padLeft - padRight);
  const innerH = wls.length * laneH;
  const totalW = padLeft + innerW + padRight;
  const totalH = padTop + innerH + padBot;

  const laneIdx  = id => wls.findIndex(w => w.id === id);
  const yForLane = i => padTop + (i + 0.5) * laneH;
  const xFor     = o => padLeft + ((o - minOrder) / orderRange) * innerW;

  host.innerHTML = '';
  const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
  svg.setAttribute('class', 'timeline-svg');
  svg.setAttribute('width', totalW);
  svg.setAttribute('height', totalH);
  svg.setAttribute('viewBox', `0 0 ${totalW} ${totalH}`);

  // Order axis ticks (top)
  const tickEvery = orderRange > 80 ? 20 : orderRange > 30 ? 10 : orderRange > 10 ? 5 : 1;
  for (let o = Math.ceil(minOrder/tickEvery)*tickEvery; o <= maxOrder; o += tickEvery) {
    const x = xFor(o);
    const t = document.createElementNS('http://www.w3.org/2000/svg', 'text');
    t.setAttribute('x', x); t.setAttribute('y', padTop - 18);
    t.setAttribute('class', 'tl-axis-label');
    t.setAttribute('text-anchor', 'middle');
    t.textContent = String(o);
    svg.appendChild(t);
    const tick = document.createElementNS('http://www.w3.org/2000/svg', 'line');
    tick.setAttribute('x1', x); tick.setAttribute('x2', x);
    tick.setAttribute('y1', padTop - 10); tick.setAttribute('y2', padTop - 4);
    tick.setAttribute('stroke', '#1a1f3a'); tick.setAttribute('stroke-width', 1);
    svg.appendChild(tick);
  }

  // Lanes (horizontal lines + labels). Per-row stroke colour is set via
  // `element.style.stroke = ...` rather than `setAttribute('stroke', ...)`:
  // CSS class rules (`.tl-lane-line { stroke: ... }`) outrank SVG
  // presentation attributes set with setAttribute, but they DO lose to
  // inline styles, so this is the only path that lets the per-worldline
  // colour actually paint the lane.
  wls.forEach((w, i) => {
    const y = yForLane(i);
    const ln = document.createElementNS('http://www.w3.org/2000/svg', 'line');
    ln.setAttribute('x1', padLeft); ln.setAttribute('x2', padLeft + innerW);
    ln.setAttribute('y1', y); ln.setAttribute('y2', y);
    ln.setAttribute('class', 'tl-lane-line' + (w.isMain ? ' is-main' : ''));
    if (w.color) ln.style.stroke = w.color;
    svg.appendChild(ln);

    const sw = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
    sw.setAttribute('x', padLeft - 180); sw.setAttribute('y', y - 6);
    sw.setAttribute('width', 12); sw.setAttribute('height', 12);
    sw.setAttribute('rx', 3);
    sw.setAttribute('fill', w.color || '#3d4670');
    svg.appendChild(sw);

    const head = document.createElementNS('http://www.w3.org/2000/svg', 'text');
    head.setAttribute('x', padLeft - 162); head.setAttribute('y', y - 2);
    head.setAttribute('class', 'tl-lane-header');
    head.textContent = w.line === '?' ? 'Unassigned' : ('Worldline ' + w.line + (w.isMain ? ' ★' : ''));
    svg.appendChild(head);

    if (w.name && w.name !== ('Worldline '+w.line)) {
      const sub = document.createElementNS('http://www.w3.org/2000/svg', 'text');
      sub.setAttribute('x', padLeft - 162); sub.setAttribute('y', y + 12);
      sub.setAttribute('class', 'tl-axis-label');
      sub.textContent = w.name.length > 24 ? w.name.slice(0, 24) + '…' : w.name;
      svg.appendChild(sub);
    }
  });

  // Group jumps by source→target lane pair. When two or more jumps share
  // the same lane pair (e.g. both regressions out of 1863rd into 1864th)
  // their endpoints are close enough that without staggering they render
  // as one arc with two labels stacked on top of each other. We push each
  // sibling further out from the straight line and lift its label more,
  // so they read as distinct curves with distinct labels.
  const jumpPairIdx = new Map();
  {
    const seen = new Map();
    jps.forEach(j => {
      const key = j.srcWl + '→' + j.tgtWl;
      const i = seen.get(key) ?? 0;
      jumpPairIdx.set(j.id, i);
      seen.set(key, i + 1);
    });
  }

  // Jumps (under events; bowed slightly perpendicular to the line so crossing
  // paths separate visually instead of stacking into one straight blob)
  jps.forEach(j => {
    const srcIdx = laneIdx(j.srcWl), tgtIdx = laneIdx(j.tgtWl);
    if (srcIdx === -1 || tgtIdx === -1) return;
    const x1 = xFor(j.srcOrder), y1 = yForLane(srcIdx);
    const x2 = xFor(j.tgtOrder), y2 = yForLane(tgtIdx);
    const dx = x2 - x1, dy = y2 - y1;
    const len = Math.sqrt(dx*dx + dy*dy) || 1;
    const siblingIdx = jumpPairIdx.get(j.id) || 0;
    // Bow proportional to arc length so a regression jump that spans the
    // whole lane reads as a curve, not a straight line. Capped so short
    // arcs don't collapse and very long ones don't balloon off the lane
    // band. Extra magnitude per sibling so co-located jumps fan apart.
    const baseOffset = Math.min(80, Math.max(18, len * 0.04));
    const offset = baseOffset + siblingIdx * 36;
    const cx = (x1 + x2) / 2 + (-dy / len) * offset;
    const cy = (y1 + y2) / 2 + ( dx / len) * offset;

    const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
    path.setAttribute('d', `M ${x1} ${y1} Q ${cx} ${cy} ${x2} ${y2}`);
    path.setAttribute('class', 'tl-jump');
    const srcWl = wls[srcIdx];
    // Inline style required so the source-lane colour beats the CSS default
    // (`stroke: var(--purple)`); see the lane-line block above for the rule.
    if (srcWl.color) path.style.stroke = srcWl.color;
    path.addEventListener('mouseenter', ev => showJumpTooltip(ev, j));
    path.addEventListener('mouseleave', hideTooltip);
    path.addEventListener('mousemove', ev => moveTooltip(ev));
    svg.appendChild(path);

    const tip = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
    tip.setAttribute('cx', x2); tip.setAttribute('cy', y2);
    tip.setAttribute('r', 3);
    tip.setAttribute('class', 'tl-jump-tip');
    if (srcWl.color) tip.style.fill = srcWl.color;
    svg.appendChild(tip);

    // Character label always above the arc midpoint. For a quadratic Bezier
    // the visual midpoint at t=0.5 is 0.25*P0 + 0.5*P1 + 0.25*P2 — not the
    // straight-line midpoint between the endpoints — so use that point and
    // lift the label 10 px above it on the canvas (smaller y = up in SVG).
    if (j.characterLabel) {
      const mx = 0.25 * x1 + 0.5 * cx + 0.25 * x2;
      const my = 0.25 * y1 + 0.5 * cy + 0.25 * y2;
      const t = document.createElementNS('http://www.w3.org/2000/svg', 'text');
      // Lift labels of siblings progressively higher so co-located jumps
      // get distinct label slots — each step is slightly more than one
      // text line-height (11 px font + 3 px room).
      t.setAttribute('x', mx); t.setAttribute('y', my - 10 - siblingIdx * 16);
      t.setAttribute('class', 'tl-jump-label');
      t.setAttribute('text-anchor', 'middle');
      // Inline style for the same CSS-precedence reason as the lane lines.
      if (srcWl.color) t.style.fill = srcWl.color;
      const label = j.characterLabel.length > 36
        ? j.characterLabel.slice(0, 34) + '…'
        : j.characterLabel;
      t.textContent = label;
      // Tooltip on the label too — users instinctively hover the text.
      t.addEventListener('mouseenter', ev => showJumpTooltip(ev, j));
      t.addEventListener('mouseleave', hideTooltip);
      t.addEventListener('mousemove', ev => moveTooltip(ev));
      svg.appendChild(t);
    }
  });

  // Events (drawn last so they sit above jumps and lane lines)
  visibleEvs.forEach(e => {
    const li = laneIdx(e.worldlineId || ORPHAN_ID);
    if (li === -1) return;
    const x = xFor(evX(e)), y = yForLane(li);
    const g = document.createElementNS('http://www.w3.org/2000/svg', 'g');
    g.setAttribute('class', 'tl-event ' + e.importance);
    g.setAttribute('transform', `translate(${x},${y})`);
    const r = e.importance === 'pivotal' ? 7 : e.importance === 'major' ? 6 : 4.5;
    const c1 = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
    c1.setAttribute('r', r);
    g.appendChild(c1);
    const lbl = document.createElementNS('http://www.w3.org/2000/svg', 'text');
    lbl.setAttribute('class', 'tl-event-label');
    lbl.setAttribute('x', 10); lbl.setAttribute('y', -8);
    lbl.textContent = e.title.length > 26 ? e.title.slice(0, 26) + '…' : e.title;
    g.appendChild(lbl);
    g.addEventListener('mouseenter', ev => showTooltip(ev, e));
    g.addEventListener('mouseleave', hideTooltip);
    g.addEventListener('mousemove', ev => moveTooltip(ev));
    svg.appendChild(g);
  });

  host.appendChild(svg);
}

let _tooltipEl = null;
function showTooltip(ev, e) {
  hideTooltip();
  const root = $('#tooltip-root');
  _tooltipEl = el('div', { class:'tooltip' },
    el('h5', {}, e.title),
    el('div', { class:'meta' }, `Ch. ${e.chapter} · ${e.importance}`),
    e.lengthEstimate ? el('div', { class:'meta' }, e.lengthEstimate) : null,
  );
  root.appendChild(_tooltipEl);
  moveTooltip(ev);
}
function showJumpTooltip(ev, j) {
  hideTooltip();
  const root = $('#tooltip-root');
  _tooltipEl = el('div', { class:'tooltip' },
    el('h5', {}, j.characterLabel || 'Worldline jump'),
    j.description ? el('div', { class:'meta' }, j.description) : null,
    j.lengthEstimate ? el('div', { class:'meta' }, j.lengthEstimate) : null,
  );
  root.appendChild(_tooltipEl);
  moveTooltip(ev);
}
function moveTooltip(ev) {
  if (!_tooltipEl) return;
  const x = ev.clientX + 14, y = ev.clientY + 14;
  _tooltipEl.style.left = x + 'px';
  _tooltipEl.style.top  = y + 'px';
  _tooltipEl.style.position = 'fixed';
}
function hideTooltip() { if (_tooltipEl) { _tooltipEl.remove(); _tooltipEl = null; } }
