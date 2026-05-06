'use strict';

/* ============================================================
   NOTIFICATIONS
============================================================ */
$('#notifBtn').addEventListener('click', openNotifDrawer);
$('#chapterPill').addEventListener('click', () => State.user ? openChapterModal() : openAuth('login'));

async function refreshUnread() {
  if (!State.user) return;
  try {
    const r = await api.get('/api/notifications/unread-count');
    State.notifUnread = r?.count ?? r?.Count ?? 0;
    $('#notifDot').style.display = State.notifUnread > 0 ? 'block' : 'none';
  } catch {}
}

async function openNotifDrawer() {
  const root = $('#drawer-root');
  root.innerHTML = '';
  const overlay = el('div', { class:'drawer-overlay', onclick:() => root.innerHTML = '' });
  const drawer  = el('div', { class:'drawer' });
  const head = el('div', { class:'drawer-head' },
    el('h3', {}, '🔔 Notifications'),
    el('button', { class:'btn btn-ghost btn-sm', onclick:async () => {
      try { await api.post('/api/notifications/read-all'); refreshUnread(); openNotifDrawer(); }
      catch (e) { toast(e.message, 'error'); }
    }}, 'Mark all read'),
  );
  const body = el('div', { class:'drawer-body' });
  drawer.appendChild(head); drawer.appendChild(body);
  root.appendChild(overlay); root.appendChild(drawer);

  body.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
  try {
    const r = await api.get('/api/notifications?page=1&pageSize=30');
    const items = r?.items || r || [];
    body.innerHTML = '';
    if (!items.length) {
      body.appendChild(el('div', { class:'empty' }, el('div', { class:'glyph' }, '🔕'), el('p', {}, 'No notifications yet.')));
      return;
    }
    items.forEach(n => {
      const isRead = n.isRead ?? n.is_read;
      const it = el('div', { class:'notif-item'+(!isRead ? ' unread' : ''), onclick:async () => {
        if (!isRead) {
          try { await api.post('/api/notifications/'+n.id+'/read'); it.classList.remove('unread'); refreshUnread(); } catch {}
        }
      }});
      it.appendChild(el('div', { class:'type' }, (n.type || n.Type || 'system').replace(/_/g, ' ')));
      const payload = n.payload || n.Payload || {};
      const text = payload.message || payload.body || payload.title || JSON.stringify(payload);
      it.appendChild(el('div', { class:'body' }, text));
      it.appendChild(el('div', { class:'time' }, fmtDate(n.createdAt || n.created_at)));
      body.appendChild(it);
    });
  } catch (e) { body.innerHTML = ''; body.appendChild(errorBlock(e)); }
}

/* Background polling */
setInterval(() => { if (State.user) refreshUnread(); }, 60000);

/* ============================================================
   SEARCH
============================================================ */
$('#searchInput').addEventListener('input', e => {
  clearTimeout(State.searchTimeout);
  const q = e.target.value.trim();
  const rEl = $('#searchResults');
  rEl.innerHTML = '';
  if (!q) return;
  State.searchTimeout = setTimeout(() => searchRun(q, rEl), 280);
});
$('#searchInput').addEventListener('blur', () => setTimeout(() => $('#searchResults').innerHTML = '', 200));

async function searchRun(q, rEl) {
  rEl.innerHTML = '<div class="search-results"><div style="padding:.6rem .8rem;color:var(--text-3)">Searching…</div></div>';
  try {
    // The backend doesn't expose a search endpoint per docs; we approximate by listing pages and
    // filtering client-side. Real prod would have /api/pages?search=...
    // Title and ShortDescription come back as RenderedContent — we flatten via
    // plainTextOf so the substring match also peeks through revealed spoilers
    // without leaking hidden-segment placeholders.
    const r = await api.get('/api/pages?page=1&pageSize=50');
    const ql = q.toLowerCase();
    const items = (r?.items || r || []).filter(p => {
      const title = plainTextOf(p.title).toLowerCase();
      const desc  = plainTextOf(p.shortDescription || p.short_description).toLowerCase();
      return title.includes(ql) || (p.slug || '').toLowerCase().includes(ql) || desc.includes(ql);
    }).slice(0, 10);
    rEl.innerHTML = '';
    if (!items.length) {
      rEl.appendChild(el('div', { class:'search-results' }, el('div', { style:{ padding:'.6rem .8rem', color:'var(--text-3)' }}, 'No matches in visible pages.')));
      return;
    }
    const wrap = el('div', { class:'search-results' });
    items.forEach(p => {
      const type = p.entityType || p.entity_type;
      const info = ENTITY_TYPES[type] || { glyph:'?', single:'Page' };
      const route = type === 'character' ? 'character/'+p.slug : 'page/'+p.slug;
      const titleNode = el('div', {}); renderRendered(p.title, titleNode);
      wrap.appendChild(el('div', { class:'search-result', onmousedown:() => { navigate(route); $('#searchInput').value=''; rEl.innerHTML=''; }},
        el('span', { style:{ color:'var(--gold)', fontSize:'1.1rem' }}, info.glyph),
        el('div', {},
          titleNode,
          el('div', { class:'meta' }, info.single + ' · Ch. ' + (p.discoveryChapter ?? p.discovery_chapter)),
        ),
      ));
    });
    rEl.appendChild(wrap);
  } catch (e) {
    rEl.innerHTML = '';
    rEl.appendChild(el('div', { class:'search-results' }, el('div', { style:{ padding:'.6rem .8rem', color:'var(--danger)' }}, e.message)));
  }
}
