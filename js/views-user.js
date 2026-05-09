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

async function renderQueue() {
  if (!State.user) return openAuth('login'), navigate('');
  const app = $('#app');
  app.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
  try {
    const r = await api.get('/api/edit-suggestions?status=pending&page=1&pageSize=30');
    const items = r?.items || r || [];
    app.innerHTML = '';
    const v = el('div', { class:'view' });
    v.appendChild(crumbs([['', 'Archive'], [null, 'Suggestion Queue']]));
    v.appendChild(el('div', { class:'detail-header' },
      el('div', { class:'type' }, '⚖ Editor Review'),
      el('h1', {}, 'Pending Edit Suggestions'),
      el('div', { class:'summary' }, 'Approve or reject reader-submitted edits. Editor or Admin role required.'),
    ));
    v.appendChild(suggestionsList(items, true));
    app.appendChild(v);
  } catch (e) { app.innerHTML = ''; app.appendChild(errorBlock(e)); }
}

function suggestionsList(items, asReviewer=false) {
  if (!items.length) return el('div', { class:'empty' }, el('div', { class:'glyph' }, '✎'), el('h3', {}, 'Nothing to show'), el('p', {}, asReviewer ? 'No pending suggestions.' : 'You have not submitted any edits yet.'));
  const list = el('div', { style:{ display:'flex', flexDirection:'column', gap:'.6rem' }});
  items.forEach(s => {
    const status = (s.status || s.Status || 'pending').toLowerCase();
    const isMine = State.user && (s.userId ?? s.user_id) === State.user.id;
    const titleText = s.pageTitle || s.PageTitle || ('Page #' + (s.pageId ?? s.page_id));
    const slug = s.pageSlug || s.page_slug;
    const titleNode = slug
      ? el('a', { href:'#/page/'+slug }, titleText)
      : titleText;
    const card = el('div', { class:'detail-body', style:{ padding:'1rem 1.2rem', margin:0 }},
      el('div', { style:{ display:'flex', justifyContent:'space-between', alignItems:'flex-start', flexWrap:'wrap', gap:'.6rem' }},
        el('div', {},
          el('h4', { style:{ marginBottom:'.2rem' }}, titleNode),
          el('div', { style:{ fontSize:'.78rem', color:'var(--text-3)' }},
            'Submitted ', fmtDate(s.createdAt || s.created_at),
            (s.username ? ' by ' + s.username : ''),
          ),
        ),
        el('span', { class:'status-badge status-'+status }, status),
      ),
      s.reason ? el('p', { style:{ marginTop:'.6rem', fontStyle:'italic', color:'var(--text-2)' }}, '"' + s.reason + '"') : null,
      el('pre', { style:{ marginTop:'.6rem', padding:'.6rem .8rem', background:'var(--space)', borderRadius:'5px', fontSize:'.8rem', overflowX:'auto', color:'var(--cyan)', border:'1px solid var(--border)' }},
        JSON.stringify(s.proposedChanges || s.proposed_changes || {}, null, 2)),
      (asReviewer && status === 'pending') || isMine ? el('div', { style:{ marginTop:'.8rem', display:'flex', gap:'.4rem', flexWrap:'wrap' }},
        asReviewer && status === 'pending' ? el('button', { class:'btn btn-primary btn-sm', onclick:async () => {
          try { await api.post('/api/edit-suggestions/'+s.id+'/approve'); toast('Approved', 'success'); renderQueue(); }
          catch (e) { toast(e.message, 'error'); }
        }}, '✓ Approve') : null,
        asReviewer && status === 'pending' ? el('button', { class:'btn btn-danger btn-sm', onclick:async () => {
          try { await api.post('/api/edit-suggestions/'+s.id+'/reject'); toast('Rejected'); renderQueue(); }
          catch (e) { toast(e.message, 'error'); }
        }}, '✕ Reject') : null,
        isMine ? el('button', { class:'btn btn-ghost btn-sm', onclick:async () => {
          if (!confirm('Delete this suggestion? This cannot be undone.')) return;
          try {
            await api.del('/api/edit-suggestions/'+s.id);
            toast('Deleted');
            asReviewer ? renderQueue() : renderMySuggestions();
          } catch (e) { toast(e.message, 'error'); }
        }}, '🗑 Delete') : null,
      ) : null,
    );
    list.appendChild(card);
  });
  return list;
}

/* ============================================================
   VIEW: TIMELINE
============================================================ */
async function renderTimeline() {
  const app = $('#app');
  app.innerHTML = '';
  const v = el('div', { class:'view' });
  v.appendChild(crumbs([['', 'Archive'], [null, 'Timeline']]));
  v.appendChild(el('div', { class:'detail-header' },
    el('div', { class:'type' }, '∞  3D Timeline'),
    el('h1', {}, 'The Worldlines'),
    el('div', { class:'summary' }, 'Parallel regression chains plotted against story chapter, with cross-line connections. ⚠ The timeline is inherently spoiler-rich. Use the chapter cap below to self-impose limits.'),
  ));

  const chapCapIn = el('input', { type:'number', placeholder:'all', min:'1', style:{ width:'90px' }, value: State.user?.currentChapter ?? '' });
  const charIdIn  = el('input', { type:'number', placeholder:'(optional)', min:'1', style:{ width:'120px' }});
  const reload = () => loadAndRender();
  v.appendChild(el('div', { class:'timeline-controls' },
    el('label', {}, 'Chapter cap'), chapCapIn,
    el('label', { style:{ marginLeft:'.6rem' }}, 'Filter by character ID'), charIdIn,
    el('button', { class:'btn btn-secondary btn-sm', onclick:reload }, 'Apply'),
    el('button', { class:'btn btn-ghost btn-sm', onclick:() => { chapCapIn.value=''; charIdIn.value=''; reload(); }}, 'Reset'),
  ));

  const canvasWrap = el('div', { class:'timeline-canvas', id:'tl-canvas' });
  const legend = el('div', { class:'tl-legend' },
    el('span', { class:'tl-legend-item' }, el('span', { class:'tl-legend-swatch', style:{ background:'var(--gold)' }}), 'Regression'),
    el('span', { class:'tl-legend-item' }, el('span', { class:'tl-legend-swatch', style:{ background:'var(--purple)' }}), 'Migration'),
    el('span', { class:'tl-legend-item' }, el('span', { class:'tl-legend-swatch', style:{ background:'var(--cyan)' }}), 'Causality'),
    el('span', { class:'tl-legend-item' }, el('span', { class:'tl-legend-swatch', style:{ background:'var(--crimson)' }}), 'Prophecy'),
    el('span', { class:'tl-legend-item' }, el('span', { class:'tl-legend-swatch', style:{ background:'var(--text-3)' }}), 'Parallel'),
    el('span', { class:'tl-legend-item' }, el('span', { style:{ display:'inline-block', width:10, height:10, borderRadius:'50%', background:'var(--gold-bright)', boxShadow:'0 0 6px var(--gold-glow)' }}), 'Pivotal event'),
    el('span', { class:'tl-legend-item' }, el('span', { style:{ display:'inline-block', width:8, height:8, borderRadius:'50%', background:'var(--gold)' }}), 'Major event'),
  );
  v.appendChild(canvasWrap);
  v.appendChild(legend);
  app.appendChild(v);

  async function loadAndRender() {
    canvasWrap.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
    let qs = [];
    if (chapCapIn.value) qs.push('upToChapter='+parseInt(chapCapIn.value));
    if (charIdIn.value) qs.push('characterId='+parseInt(charIdIn.value));
    const url = '/api/timeline' + (qs.length ? '?' + qs.join('&') : '');
    try {
      const data = await api.get(url);
      drawTimeline(canvasWrap, data);
    } catch (e) { canvasWrap.innerHTML = ''; canvasWrap.appendChild(errorBlock(e)); }
  }
  loadAndRender();
}

function drawTimeline(host, data) {
  const worldlines = data?.worldlines || data?.Worldlines || [];
  const events     = data?.events     || data?.Events     || [];
  const conns      = data?.connections|| data?.Connections|| [];

  if (worldlines.length === 0 && events.length === 0) {
    host.innerHTML = '';
    host.appendChild(el('div', { class:'empty' }, el('div', { class:'glyph' }, '∞'), el('h3', {}, 'No timeline data yet'), el('p', {}, 'Once events are connected to worldlines, this view comes alive.')));
    return;
  }

  // Normalize
  const wls = worldlines.map(w => ({
    id: w.id ?? w.Id,
    line: w.lineNumber ?? w.line_number ?? w.LineNumber ?? 0,
    name: w.name ?? w.Name ?? null,
    isMain: w.isMain ?? w.is_main ?? w.IsMain ?? false,
  }));
  // Sort by lineNumber, with line 0 at the start, then ascending
  wls.sort((a,b) => a.line - b.line);
  const evs = events.map(e => ({
    id: e.id ?? e.Id,
    title: e.title ?? e.Title ?? '',
    chapter: e.chapterNumber ?? e.chapter_number ?? e.ChapterNumber ?? 1,
    worldlineId: e.worldlineId ?? e.worldline_id ?? e.WorldlineId,
    importance: (e.importance ?? e.Importance ?? 'minor').toLowerCase(),
  }));
  const cns = conns.map(c => ({
    src: c.sourceEventId ?? c.source_event_id ?? c.SourceEventId,
    tgt: c.targetEventId ?? c.target_event_id ?? c.TargetEventId,
    type:(c.connectionType ?? c.connection_type ?? c.ConnectionType ?? 'parallel').toLowerCase(),
  }));

  // Add a synthetic lane for events without a worldline
  const ORPHAN_ID = '__orphan__';
  if (evs.some(e => !e.worldlineId)) {
    wls.push({ id: ORPHAN_ID, line: '?', name: 'Unassigned', isMain:false });
  }

  // Layout
  const laneW = 220;
  const padTop = 80, padBot = 60, padLeft = 80, padRight = 40;
  const minChap = Math.min(1, ...evs.map(e => e.chapter));
  const maxChap = Math.max(50, ...evs.map(e => e.chapter));
  const chapRange = Math.max(1, maxChap - minChap);
  const heightPerChapter = 6;
  const innerH = Math.max(420, chapRange * heightPerChapter);
  const totalH = innerH + padTop + padBot;
  const totalW = padLeft + wls.length * laneW + padRight;

  const laneX = id => {
    const i = wls.findIndex(w => w.id === id);
    return padLeft + (i + 0.5) * laneW;
  };
  const yFor = ch => padTop + ((ch - minChap) / chapRange) * innerH;

  host.innerHTML = '';
  const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
  svg.setAttribute('class', 'timeline-svg');
  svg.setAttribute('width', totalW);
  svg.setAttribute('height', totalH);
  svg.setAttribute('viewBox', `0 0 ${totalW} ${totalH}`);

  // Chapter axis
  const tickEvery = chapRange > 800 ? 100 : chapRange > 400 ? 50 : chapRange > 100 ? 25 : chapRange > 30 ? 10 : 5;
  for (let c = Math.ceil(minChap/tickEvery)*tickEvery; c <= maxChap; c += tickEvery) {
    const y = yFor(c);
    const ln = document.createElementNS('http://www.w3.org/2000/svg', 'line');
    ln.setAttribute('x1', padLeft - 30); ln.setAttribute('x2', totalW - padRight + 10);
    ln.setAttribute('y1', y); ln.setAttribute('y2', y);
    ln.setAttribute('stroke', '#1a1f3a'); ln.setAttribute('stroke-width', 1);
    svg.appendChild(ln);
    const t = document.createElementNS('http://www.w3.org/2000/svg', 'text');
    t.setAttribute('x', padLeft - 36); t.setAttribute('y', y+3);
    t.setAttribute('class', 'tl-axis-label'); t.setAttribute('text-anchor', 'end');
    t.textContent = 'Ch ' + c;
    svg.appendChild(t);
  }

  // Lane lines and headers
  wls.forEach(w => {
    const x = laneX(w.id);
    const ln = document.createElementNS('http://www.w3.org/2000/svg', 'line');
    ln.setAttribute('x1', x); ln.setAttribute('x2', x);
    ln.setAttribute('y1', padTop - 20); ln.setAttribute('y2', totalH - padBot + 10);
    ln.setAttribute('class', 'tl-lane-line');
    if (w.isMain) ln.setAttribute('stroke', '#3d4670');
    svg.appendChild(ln);
    const head = document.createElementNS('http://www.w3.org/2000/svg', 'text');
    head.setAttribute('x', x); head.setAttribute('y', 28);
    head.setAttribute('class', 'tl-lane-header'); head.setAttribute('text-anchor', 'middle');
    head.textContent = w.line === '?' ? 'Unassigned' : ('Worldline ' + w.line + (w.isMain ? ' ★' : ''));
    svg.appendChild(head);
    if (w.name && w.name !== ('Worldline '+w.line)) {
      const sub = document.createElementNS('http://www.w3.org/2000/svg', 'text');
      sub.setAttribute('x', x); sub.setAttribute('y', 46);
      sub.setAttribute('class', 'tl-axis-label'); sub.setAttribute('text-anchor', 'middle');
      sub.textContent = w.name.length > 22 ? w.name.slice(0, 22) + '…' : w.name;
      svg.appendChild(sub);
    }
  });

  // Connections (curves between events on possibly different lanes)
  const eventById = {}; evs.forEach(e => { eventById[e.id] = e; });
  cns.forEach(c => {
    const s = eventById[c.src], t = eventById[c.tgt];
    if (!s || !t) return;
    const x1 = laneX(s.worldlineId || ORPHAN_ID), y1 = yFor(s.chapter);
    const x2 = laneX(t.worldlineId || ORPHAN_ID), y2 = yFor(t.chapter);
    const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
    const cx = (x1 + x2) / 2 + (x2 > x1 ? 30 : -30);
    const cy = (y1 + y2) / 2;
    const d = `M ${x1} ${y1} Q ${cx} ${cy} ${x2} ${y2}`;
    path.setAttribute('d', d);
    path.setAttribute('class', 'tl-conn ' + c.type);
    svg.appendChild(path);
    // Arrowhead via small circle
    const tip = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
    tip.setAttribute('cx', x2); tip.setAttribute('cy', y2);
    tip.setAttribute('r', 2.5);
    tip.setAttribute('fill', c.type === 'regression' ? 'var(--gold)' : c.type === 'causality' ? 'var(--cyan)' : c.type === 'prophecy' ? 'var(--crimson)' : 'var(--purple)');
    svg.appendChild(tip);
  });

  // Events
  evs.forEach(e => {
    const x = laneX(e.worldlineId || ORPHAN_ID), y = yFor(e.chapter);
    const g = document.createElementNS('http://www.w3.org/2000/svg', 'g');
    g.setAttribute('class', 'tl-event ' + e.importance);
    g.setAttribute('transform', `translate(${x},${y})`);
    const r = e.importance === 'pivotal' ? 7 : e.importance === 'major' ? 6 : 4.5;
    const c1 = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
    c1.setAttribute('r', r);
    g.appendChild(c1);
    const lbl = document.createElementNS('http://www.w3.org/2000/svg', 'text');
    lbl.setAttribute('class', 'tl-event-label');
    lbl.setAttribute('x', 12); lbl.setAttribute('y', 4);
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
