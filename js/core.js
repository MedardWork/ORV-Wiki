'use strict';

/* ============================================================
   STATE
============================================================ */
const State = {
  // Persisted preferences are loaded from localStorage on first paint and
  // re-saved whenever they change (see savePrefs()). The default API base
  // depends on where the page is being served from:
  //   file://             → local Kestrel dev URL
  //   localhost (docker)  → '' (relative; nginx reverse-proxies /api & /hubs)
  //   anywhere else       → window.ORV_API_BASE from js/config.js
  //                          (GitHub Pages, Cloudflare, custom domain, …)
  // localStorage overrides the default — EXCEPT when a stale localhost URL
  // is saved and we're on a non-local origin. That happens when a user
  // tested locally then opened the public site: their browser still has
  // `orv.apiBase = https://localhost:7138` and the site can't reach it.
  apiBase: (() => {
    const h = location.hostname;
    const isLocalOrigin = location.protocol === 'file:'
      || h === 'localhost' || h === '127.0.0.1' || h === '0.0.0.0';
    const stored = localStorage.getItem('orv.apiBase');
    const storedLooksLocal = stored && /^(https?:\/\/)?(localhost|127\.0\.0\.1|0\.0\.0\.0)/.test(stored);

    if (stored !== null && !(storedLooksLocal && !isLocalOrigin)) return stored;
    if (location.protocol === 'file:') return 'https://localhost:7138';
    if (isLocalOrigin) return '';
    return window.ORV_API_BASE || '';
  })(),
  ignoreSpoilers: localStorage.getItem('orv.ignoreSpoilers') === '1',
  token: null,
  user: null,
  view: { type: 'home', params: {} },
  notifUnread: 0,
  searchTimeout: null,
};

function savePrefs() {
  localStorage.setItem('orv.apiBase', State.apiBase);
  localStorage.setItem('orv.ignoreSpoilers', State.ignoreSpoilers ? '1' : '0');
}

/* ============================================================
   CONFIG
   ENTITY_TYPES drives the menubar, page-card glyphs, and the
   "fetch the entity-specific endpoint to surface narrative fields"
   logic in renderPage(). `endpoint` is a base path that supports
   `/by-slug/{slug}`; `narratives` lists the RenderedContent fields
   on the entity DTO that should appear under their own heading.
============================================================ */
const ENTITY_TYPES = {
  character:   { plural:'Characters',   single:'Character',     glyph:'☉', desc:'Incarnations and mortals across the worldlines',
                 endpoint:'/api/characters', narratives:[['biography','Biography']] },
  constellation:{plural:'Constellations',single:'Constellation', glyph:'✦', desc:'Beings of the Star Stream',
                 endpoint:'/api/constellations', narratives:[['description','Description']] },
  nebula:      { plural:'Nebulae',      single:'Nebula',        glyph:'✺', desc:'Constellation factions and alliances',
                 endpoint:'/api/nebulae', narratives:[['description','Description']] },
  dokkaebi:    { plural:'Dokkaebi',     single:'Dokkaebi',      glyph:'☄', desc:'Goblin scenario administrators',
                 endpoint:'/api/dokkaebi', narratives:[['speciality','Speciality']] },
  demon_king:  { plural:'Demon Kings',  single:'Demon King',    glyph:'☠', desc:'The 72 sovereigns of the underworld',
                 endpoint:'/api/demon-kings', narratives:[['description','Description']] },
  outer_god:   { plural:'Outer Gods',   single:'Outer God',     glyph:'◉', desc:'Beings beyond the Star Stream',
                 endpoint:'/api/outer-gods', narratives:[['description','Description']] },
  worldline:   { plural:'Worldlines',   single:'Worldline',     glyph:'∞', desc:'Parallel regression chains',
                 endpoint:'/api/worldlines', narratives:[['description','Description']] },
  fable:       { plural:'Fables',       single:'Fable',         glyph:'❦', desc:'Stories that grant power',
                 endpoint:'/api/fables', narratives:[['title','Title'],['legend','Legend']] },
  stigma:      { plural:'Stigmas',      single:'Stigma',        glyph:'✧', desc:'Powers loaned by sponsoring constellations',
                 endpoint:'/api/stigmas', narratives:[['effect','Effect']] },
  attribute:   { plural:'Attributes',   single:'Attribute',     glyph:'◈', desc:'Innate personal traits',
                 endpoint:'/api/attributes', narratives:[['effect','Effect']] },
  skill:       { plural:'Skills',       single:'Skill',         glyph:'➤', desc:'Active and passive abilities',
                 endpoint:'/api/skills', narratives:[['effect','Effect']] },
  item:        { plural:'Items',        single:'Item',          glyph:'⚔', desc:'Star relics and equipment',
                 endpoint:'/api/items', narratives:[['description','Description']] },
  location:    { plural:'Locations',    single:'Location',      glyph:'⌖', desc:'Places across worldlines and dimensions',
                 endpoint:'/api/locations', narratives:[['description','Description']] },
  scenario:    { plural:'Scenarios',    single:'Scenario',      glyph:'☰', desc:'Main, sub, hidden and bounty scenarios',
                 endpoint:'/api/scenarios', narratives:[['title','Title'],['conditions','Conditions'],['rewards','Rewards'],['penalty','Penalty']] },
  arc:         { plural:'Arcs',         single:'Arc',           glyph:'◐', desc:'Story arcs of the novel',
                 endpoint:'/api/arcs', narratives:[['summary','Summary']] },
  chapter:     { plural:'Chapters',     single:'Chapter',       glyph:'📜', desc:'Individual novel chapters' },
  event:       { plural:'Events',       single:'Event',         glyph:'⚡', desc:'Pivotal moments in the timeline',
                 endpoint:'/api/events', narratives:[['title','Title'],['description','Description']] },
  concept:     { plural:'Concepts',     single:'Concept',       glyph:'◇', desc:'Abstract ideas and rules of the world',
                 endpoint:'/api/concepts', narratives:[['definition','Definition']] },
};

const REACTIONS = [
  { type:'like',    emoji:'👍' },
  { type:'dislike', emoji:'👎' },
  { type:'heart',   emoji:'❤️' },
  { type:'laugh',   emoji:'😂' },
  { type:'sad',     emoji:'😢' },
  { type:'star',    emoji:'⭐' },
];

const MENU_GROUPS = [
  { label:'Beings',         types:['character','constellation','nebula','dokkaebi','demon_king','outer_god','worldline'] },
  { label:'Power System',   types:['fable','stigma','attribute','skill','item'] },
  { label:'Story & World',  types:['location','scenario','arc','chapter','event','concept'] },
];

/* ============================================================
   API CLIENT
============================================================ */
const api = {
  async req(method, path, body) {
    // When the user has the spoiler gate disabled, every page-related GET
    // gets ?ignoreSpoilers=true tacked on — the backend then bypasses the
    // discovery_chapter filter regardless of the JWT's current_chapter claim.
    let finalPath = path;
    if (method === 'GET' && State.ignoreSpoilers
        && (path.startsWith('/api/pages') || path.startsWith('/api/characters'))) {
      finalPath += (path.includes('?') ? '&' : '?') + 'ignoreSpoilers=true';
    }
    const url = State.apiBase.replace(/\/$/, '') + finalPath;
    const headers = { 'Accept':'application/json' };
    if (body !== undefined) headers['Content-Type'] = 'application/json';
    if (State.token) headers['Authorization'] = 'Bearer ' + State.token;
    let res;
    try {
      res = await fetch(url, { method, headers, body: body !== undefined ? JSON.stringify(body) : undefined });
    } catch (e) {
      throw new ApiError(0, 'Cannot reach the Star Stream — check your API base URL in settings.');
    }
    if (res.status === 204) return null;
    let data = null;
    const ct = res.headers.get('content-type') || '';
    if (ct.includes('json')) {
      try { data = await res.json(); } catch {}
    }
    if (!res.ok) {
      // Prefer the specific field errors over the generic problem title
      // ("Validation failed.") so the user sees what actually went wrong.
      const errs = data?.errors ? Object.values(data.errors).flat().join('; ') : null;
      const msg = errs || data?.detail || data?.title || res.statusText || 'Request failed';
      throw new ApiError(res.status, msg, data);
    }
    return data;
  },
  get(p)        { return this.req('GET', p); },
  post(p, b)    { return this.req('POST', p, b ?? {}); },
  put(p, b)     { return this.req('PUT', p, b); },
  patch(p, b)   { return this.req('PATCH', p, b ?? {}); },
  del(p)        { return this.req('DELETE', p); },
};
class ApiError extends Error { constructor(s, m, d){ super(m); this.status=s; this.data=d; } }

/* ============================================================
   UTILITIES
============================================================ */
const $  = (s, r=document) => r.querySelector(s);
const $$ = (s, r=document) => Array.from(r.querySelectorAll(s));
const el = (tag, props={}, ...kids) => {
  const e = document.createElement(tag);
  for (const [k,v] of Object.entries(props)) {
    if (k === 'class') e.className = v;
    else if (k === 'style') Object.assign(e.style, v);
    else if (k === 'dataset') Object.assign(e.dataset, v);
    else if (k.startsWith('on') && typeof v === 'function') e.addEventListener(k.slice(2).toLowerCase(), v);
    else if (k === 'html') e.innerHTML = v;
    else if (v !== null && v !== undefined && v !== false) e.setAttribute(k, v);
  }
  for (const k of kids.flat()) {
    if (k == null || k === false) continue;
    e.appendChild(typeof k === 'string' || typeof k === 'number' ? document.createTextNode(k) : k);
  }
  return e;
};
const escapeHtml = s => String(s ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const fmtDate = s => {
  if (!s) return '—';
  const d = new Date(s);
  const diff = Date.now() - d.getTime();
  const min = 60*1000, h = 60*min, day = 24*h;
  if (diff < min) return 'just now';
  if (diff < h)   return Math.floor(diff/min) + 'm ago';
  if (diff < day) return Math.floor(diff/h)   + 'h ago';
  if (diff < 7*day) return Math.floor(diff/day)+ 'd ago';
  return d.toLocaleDateString();
};
const decodeJwt = t => {
  try {
    const p = t.split('.')[1];
    return JSON.parse(atob(p.replace(/-/g,'+').replace(/_/g,'/')));
  } catch { return null; }
};

/* ============================================================
   TOAST
============================================================ */
function toast(msg, type='info', ttl=3500) {
  const t = el('div', { class:'toast '+type }, msg);
  $('#toast-root').appendChild(t);
  setTimeout(() => { t.style.opacity='0'; t.style.transform='translateX(40px)'; t.style.transition='all .25s'; }, ttl);
  setTimeout(() => t.remove(), ttl + 300);
}

/* ============================================================
   MODAL
============================================================ */
function openModal(content, opts={}) {
  closeModal();
  const root = $('#modal-root');
  const backdrop = el('div', { class:'modal-backdrop', onclick:e => { if (e.target === backdrop) closeModal(); }});
  const m = el('div', { class:'modal'+(opts.lg ? ' lg' : ''), style:{ position:'relative' }});
  m.appendChild(el('button', { class:'modal-close', onclick:closeModal, 'aria-label':'Close' }, '×'));
  if (typeof content === 'function') {
    const inner = content(closeModal);
    if (inner instanceof Node) m.appendChild(inner);
  } else if (content instanceof Node) m.appendChild(content);
  backdrop.appendChild(m);
  root.appendChild(backdrop);
  setTimeout(() => { const f = m.querySelector('input,textarea,button'); if (f) f.focus(); }, 50);
  return m;
}
function closeModal() { $('#modal-root').innerHTML = ''; }
