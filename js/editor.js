'use strict';

/* ============================================================
   CONTENT EDITOR
   Schema-driven create/edit forms for every content type plus
   the generalized edit-suggestion flow. The schema comes from
   /api/content-types, so there is no per-type form code.
============================================================ */

function isEditor() {
  return !!(State.user && (State.user.role === 'editor' || State.user.role === 'admin'));
}

// PascalCase enum option -> readable label ("StarStream" -> "Star Stream").
function prettyEnum(s) {
  return String(s).replace(/([a-z0-9])([A-Z])/g, '$1 $2');
}

function fieldValueEquals(a, b) {
  const emptyA = a === null || a === undefined || a === '';
  const emptyB = b === null || b === undefined || b === '';
  if (emptyA && emptyB) return true;
  return a === b;
}

/* ---- schema + ref-target caches ---------------------------- */
const _schemaCache = {};
function loadContentSchema(type) {
  if (!_schemaCache[type]) _schemaCache[type] = api.get('/api/content-types/' + type);
  return _schemaCache[type];
}

const _refCache = {};
function loadRefOptions(type) {
  if (!_refCache[type]) {
    _refCache[type] = api.get(`/api/pages?entityType=${type}&pageSize=200&ignoreSpoilers=true`)
      .then(r => (r?.items || r?.Items || []).map(p => ({
        id: p.id,
        label: plainTextOf(p.title) || p.slug,
      })))
      .catch(() => []);
  }
  return _refCache[type];
}

/* ---- field inputs ------------------------------------------ */
function buildFieldInput(field, value) {
  let input, read;
  const k = field.kind;
  if (k === 'long_text') {
    input = el('textarea', {});
    if (value != null) input.value = value;
    read = () => input.value.trim() === '' ? null : input.value;
  } else if (k === 'int') {
    input = el('input', { type:'number' });
    if (value != null) input.value = value;
    read = () => input.value === '' ? null : parseInt(input.value, 10);
  } else if (k === 'bool') {
    input = el('input', { type:'checkbox' });
    input.checked = !!value;
    read = () => input.checked;
  } else if (k === 'enum') {
    input = el('select', {});
    if (field.nullable) input.appendChild(el('option', { value:'' }, '— none —'));
    (field.enumOptions || []).forEach(o => input.appendChild(el('option', { value:o }, prettyEnum(o))));
    // A non-nullable enum has no "— none —" option, so leaving value unset lets
    // the select keep its first real option rather than going unselected.
    if (value != null) input.value = value;
    read = () => input.value === '' ? null : input.value;
  } else if (k === 'ref') {
    input = el('select', {});
    input.appendChild(el('option', { value:'' }, '— none —'));
    loadRefOptions(field.refTarget).then(opts => {
      opts.forEach(o => input.appendChild(el('option', { value:o.id }, o.label)));
      if (value != null) input.value = value;
    });
    read = () => input.value === '' ? null : parseInt(input.value, 10);
  } else {
    input = el('input', { type:'text' });
    if (value != null) input.value = value;
    if (field.maxLength) input.maxLength = field.maxLength;
    read = () => input.value.trim() === '' ? null : input.value;
  }
  return { input, read };
}

function buildFormField(field, value, readOnly) {
  const { input, read } = buildFieldInput(field, value);
  if (readOnly) input.disabled = true;
  const wrap = el('div', { class:'field' },
    el('label', {}, field.label + (field.required ? ' *' : '')),
    input,
  );
  if (field.kind === 'long_text') {
    wrap.appendChild(el('div', { style:{ fontSize:'.74rem', color:'var(--text-3)', marginTop:'.25rem' }},
      'Use [spoiler ch=N]…[/spoiler] to hide passages until chapter N.'));
  }
  return { wrap, read };
}

/* ---- relation editor --------------------------------------- */
function renderRelationEditor(rel, currentLinks) {
  const wrap = el('div', { style:{ marginTop:'1rem', paddingTop:'.7rem', borderTop:'1px solid var(--border)' }});
  wrap.appendChild(el('div', { style:{ fontFamily:'var(--serif)', color:'var(--gold)', marginBottom:'.4rem' }}, rel.label));
  const rowsWrap = el('div', {});
  wrap.appendChild(rowsWrap);
  const rows = [];

  function addRow(link, isNew) {
    const metaReaders = [];
    const metaNodes = [];
    (rel.joinFields || []).forEach(jf => {
      const { input, read } = buildFieldInput(jf, link.metadata ? link.metadata[jf.name] : null);
      metaReaders.push({ name: jf.name, read });
      metaNodes.push(el('label', { style:{ display:'flex', flexDirection:'column', fontSize:'.7rem', color:'var(--text-3)' }},
        jf.label, input));
    });
    const removeBtn = el('button', { type:'button', class:'btn btn-ghost btn-sm' }, '✕');
    const row = el('div', { style:{ display:'flex', gap:'.5rem', alignItems:'flex-end', flexWrap:'wrap',
      padding:'.4rem 0', borderBottom:'1px dashed var(--border)' }},
      el('span', { style:{ flex:'1', minWidth:'150px', fontWeight:'500' }},
        link.targetTitle || ('Page #' + link.targetPageId)),
      ...metaNodes,
      removeBtn,
    );
    const rec = { targetPageId: link.targetPageId, isNew, removed: false,
                  originalMeta: link.metadata || {}, metaReaders };
    removeBtn.addEventListener('click', () => { rec.removed = true; row.remove(); });
    rows.push(rec);
    rowsWrap.appendChild(row);
  }

  (currentLinks || []).forEach(l => addRow(l, false));

  const addSelect = el('select', { style:{ flex:'1', minWidth:'160px' }});
  addSelect.appendChild(el('option', { value:'' }, '— pick to add —'));
  loadRefOptions(rel.targetType).then(opts =>
    opts.forEach(o => addSelect.appendChild(el('option', { value:o.id }, o.label))));
  const addBtn = el('button', { type:'button', class:'btn btn-secondary btn-sm', onclick:() => {
    const id = parseInt(addSelect.value, 10);
    if (!id) return;
    if (rows.some(r => r.targetPageId === id && !r.removed)) { toast('Already linked', 'error'); return; }
    addRow({ targetPageId:id, targetTitle: addSelect.options[addSelect.selectedIndex].textContent, metadata:{} }, true);
    addSelect.value = '';
  }}, '+ Add');
  wrap.appendChild(el('div', { style:{ display:'flex', gap:'.5rem', marginTop:'.5rem', flexWrap:'wrap' }}, addSelect, addBtn));

  function collect() {
    const add = [], update = [], remove = [];
    for (const r of rows) {
      const meta = {};
      r.metaReaders.forEach(mr => { meta[mr.name] = mr.read(); });
      if (r.isNew) {
        if (!r.removed) add.push(Object.assign({ targetPageId: r.targetPageId }, meta));
      } else if (r.removed) {
        remove.push(r.targetPageId);
      } else if (r.metaReaders.some(mr => !fieldValueEquals(mr.read(), r.originalMeta[mr.name]))) {
        update.push(Object.assign({ targetPageId: r.targetPageId }, meta));
      }
    }
    return { add, update, remove };
  }

  return { node: wrap, collect };
}

/* ---- full content form ------------------------------------- */
function renderContentForm(descriptor, opts) {
  opts = opts || {};
  const mode = opts.mode || 'edit';
  const values = opts.values || {};
  const relValues = opts.relations || {};
  const form = el('div', {});
  const readers = [];

  descriptor.fields.forEach(field => {
    const readOnly = mode === 'edit' && field.createOnly;
    const { wrap, read } = buildFormField(field, values[field.name], readOnly);
    if (!readOnly) readers.push({ field, read });
    form.appendChild(wrap);
  });

  const relCollectors = [];
  (descriptor.relations || []).forEach(rel => {
    const { node, collect } = renderRelationEditor(rel, relValues[rel.name] || []);
    relCollectors.push({ rel, collect });
    form.appendChild(node);
  });

  function collect() {
    const fields = {};
    readers.forEach(({ field, read }) => {
      const cur = read();
      if (mode === 'create') {
        if (cur != null || field.required) fields[field.name] = cur;
      } else if (!fieldValueEquals(cur, values[field.name])) {
        fields[field.name] = cur;
      }
    });
    const relations = {};
    relCollectors.forEach(({ rel, collect }) => {
      const d = collect();
      if (d.add.length || d.update.length || d.remove.length) relations[rel.name] = d;
    });
    return { fields, relations };
  }

  return { node: form, collect };
}

/* ---- diff rendering (suggestion queue / history) ----------- */
function formatDiffValue(v) {
  if (v === null || v === undefined || v === '') return '(empty)';
  if (typeof v === 'boolean') return v ? 'yes' : 'no';
  const s = String(v);
  return s.length > 240 ? s.slice(0, 240) + '…' : s;
}

function renderDiff(changes) {
  const box = el('div', { style:{ marginTop:'.6rem', display:'flex', flexDirection:'column', gap:'.25rem' }});
  if (!changes || typeof changes !== 'object') {
    box.appendChild(el('span', { style:{ color:'var(--text-3)', fontStyle:'italic', fontSize:'.82rem' }}, 'No details.'));
    return box;
  }
  const hasStructure = ('fields' in changes) || ('relations' in changes);
  const fields = hasStructure ? (changes.fields || {}) : changes;
  const relations = hasStructure ? (changes.relations || {}) : {};
  const row = (key, val) => el('div', { style:{ display:'flex', gap:'.6rem', fontSize:'.82rem' }},
    el('span', { style:{ color:'var(--gold-dim)', minWidth:'140px', fontWeight:'500' }}, humanize(key)),
    el('span', { style:{ color:'var(--text)', whiteSpace:'pre-wrap', wordBreak:'break-word' }}, val));
  for (const [k, v] of Object.entries(fields)) box.appendChild(row(k, formatDiffValue(v)));
  for (const [rel, ops] of Object.entries(relations)) {
    const parts = [];
    if (ops.add?.length) parts.push('+' + ops.add.length + ' added');
    if (ops.update?.length) parts.push('~' + ops.update.length + ' changed');
    if (ops.remove?.length) parts.push('−' + ops.remove.length + ' removed');
    box.appendChild(row(rel, parts.join('  ·  ') || 'no change'));
  }
  if (!box.children.length)
    box.appendChild(el('span', { style:{ color:'var(--text-3)', fontStyle:'italic', fontSize:'.82rem' }}, 'No changes.'));
  return box;
}

/* ---- edit-suggestion modals -------------------------------- */
async function openSuggest(p) {
  if (!State.user) return openAuth('login');
  const type = p.entityType || p.entity_type;
  if (!type || !ENTITY_TYPES[type]) { toast('Unknown page type', 'error'); return; }
  let descriptor, snapshot;
  try {
    descriptor = await loadContentSchema(type);
    snapshot = await api.get(`/api/content/${type}/${p.id}`);
  } catch (e) { toast(e.message, 'error'); return; }
  openModal(close => {
    const form = renderContentForm(descriptor, {
      mode: 'edit',
      values: snapshot.fields || {},
      relations: snapshot.relations || {},
    });
    const reasonIn = el('textarea', { placeholder:'Reason / source (optional)' });
    return el('div', { class:'modal-content' },
      el('h2', {}, 'Suggest an Edit'),
      el('div', { class:'modal-sub' }, 'Editors review every proposal before it goes live. Only changed fields are submitted.'),
      form.node,
      el('div', { class:'field' }, el('label', {}, 'Reason'), reasonIn),
      el('div', { class:'modal-actions' },
        el('button', { class:'btn btn-ghost', onclick:close }, 'Cancel'),
        el('button', { class:'btn btn-primary', onclick:async ev => {
          const changes = form.collect();
          if (!Object.keys(changes.fields).length && !Object.keys(changes.relations).length) {
            toast('No changes detected', 'error'); return;
          }
          ev.target.disabled = true;
          try {
            await api.post('/api/edit-suggestions', {
              operation:'update', entityType:type, pageId:p.id,
              proposedChanges:changes, reason:reasonIn.value.trim() || null,
            });
            toast('Suggestion submitted — thank you', 'success');
            close();
          } catch (e) { toast(e.message, 'error'); ev.target.disabled = false; }
        }}, 'Submit suggestion'),
      ),
    );
  }, { lg:true });
}

async function openSuggestNew(type) {
  if (!State.user) return openAuth('login');
  if (!ENTITY_TYPES[type]) { toast('Unknown page type', 'error'); return; }
  let descriptor;
  try { descriptor = await loadContentSchema(type); }
  catch (e) { toast(e.message, 'error'); return; }
  openModal(close => {
    const form = renderContentForm(descriptor, { mode:'create' });
    const reasonIn = el('textarea', { placeholder:'Reason / source (optional)' });
    return el('div', { class:'modal-content' },
      el('h2', {}, 'Suggest a New ' + descriptor.displayName),
      el('div', { class:'modal-sub' }, 'Editors review every proposal before the page is created.'),
      form.node,
      el('div', { class:'field' }, el('label', {}, 'Reason'), reasonIn),
      el('div', { class:'modal-actions' },
        el('button', { class:'btn btn-ghost', onclick:close }, 'Cancel'),
        el('button', { class:'btn btn-primary', onclick:async ev => {
          const changes = form.collect();
          ev.target.disabled = true;
          try {
            await api.post('/api/edit-suggestions', {
              operation:'create', entityType:type, pageId:null,
              proposedChanges:changes, reason:reasonIn.value.trim() || null,
            });
            toast('New-page suggestion submitted — thank you', 'success');
            close();
          } catch (e) { toast(e.message, 'error'); ev.target.disabled = false; }
        }}, 'Submit suggestion'),
      ),
    );
  }, { lg:true });
}

/* ---- editor tool views ------------------------------------- */
function editorDenied() {
  const app = $('#app');
  app.innerHTML = '';
  app.appendChild(el('div', { class:'empty' },
    el('div', { class:'glyph' }, '⚖'),
    el('h3', {}, 'Editors only'),
    el('p', {}, 'You need the Editor or Admin role to use these tools.'),
  ));
}

async function renderEditorHome() {
  if (!isEditor()) return editorDenied();
  const app = $('#app');
  app.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
  try {
    const types = await api.get('/api/content-types');
    app.innerHTML = '';
    const v = el('div', { class:'view' });
    v.appendChild(crumbs([['', 'Archive'], [null, 'Editor Tools']]));
    v.appendChild(el('div', { class:'detail-header' },
      el('div', { class:'type' }, '✦ Editor Tools'),
      el('h1', {}, 'Manage Content'),
      el('div', { class:'summary' }, 'Create, edit and delete wiki content. Every change is recorded in the suggestion history.'),
    ));
    const grid = el('div', { class:'cat-grid' });
    types.forEach(t => {
      const info = ENTITY_TYPES[t.entityType] || { glyph:'◇' };
      grid.appendChild(el('div', { class:'cat-tile', onclick:() => navigate('editor/' + t.entityType) },
        el('span', { class:'glyph' }, info.glyph),
        el('h4', {}, t.displayName),
        el('p', {}, t.fields.length + ' fields · ' + t.relations.length + ' relations'),
      ));
    });
    v.appendChild(grid);
    app.appendChild(v);
  } catch (e) { app.innerHTML = ''; app.appendChild(errorBlock(e)); }
}

async function renderEditorType(type) {
  if (!isEditor()) return editorDenied();
  const app = $('#app');
  const info = ENTITY_TYPES[type];
  if (!info) { app.innerHTML = '<div class="empty"><h3>Unknown content type</h3></div>'; return; }
  app.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
  try {
    const r = await api.get(`/api/pages?entityType=${type}&pageSize=100&ignoreSpoilers=true`);
    const items = r?.items || r?.Items || [];
    app.innerHTML = '';
    const v = el('div', { class:'view' });
    v.appendChild(crumbs([['', 'Archive'], ['editor', 'Editor Tools'], [null, info.plural]]));
    v.appendChild(el('div', { class:'detail-header' },
      el('div', { class:'type' }, info.glyph + ' Editor'),
      el('h1', {}, 'Manage ' + info.plural),
      el('div', { class:'summary' }, info.desc + '.'),
    ));
    v.appendChild(el('div', { style:{ marginBottom:'1rem' }},
      el('button', { class:'btn btn-primary', onclick:() => navigate('editor/' + type + '/new') }, '+ New ' + info.single)));
    if (!items.length) {
      v.appendChild(el('div', { class:'empty' }, el('div', { class:'glyph' }, info.glyph),
        el('h3', {}, 'No ' + info.plural + ' yet'), el('p', {}, 'Create the first one.')));
    } else {
      const list = el('div', { style:{ display:'flex', flexDirection:'column', gap:'.4rem' }});
      items.forEach(p => {
        const title = plainTextOf(p.title) || p.slug;
        list.appendChild(el('div', { class:'detail-body', style:{ margin:0, padding:'.7rem 1rem',
          display:'flex', justifyContent:'space-between', alignItems:'center', gap:'.6rem', flexWrap:'wrap' }},
          el('div', {},
            el('strong', {}, title),
            el('span', { style:{ color:'var(--text-3)', fontSize:'.78rem', marginLeft:'.5rem' }}, '/' + p.slug)),
          el('div', { style:{ display:'flex', gap:'.4rem' }},
            el('button', { class:'btn btn-secondary btn-sm', onclick:() => navigate('editor/' + type + '/' + p.id) }, 'Edit'),
            el('button', { class:'btn btn-danger btn-sm', onclick:() => deleteContent(type, p.id, title) }, 'Delete'),
          ),
        ));
      });
      v.appendChild(list);
    }
    app.appendChild(v);
  } catch (e) { app.innerHTML = ''; app.appendChild(errorBlock(e)); }
}

async function deleteContent(type, pageId, title) {
  if (!confirm(`Delete "${title}"? This removes the page and cannot be undone.`)) return;
  try {
    await api.del(`/api/content/${type}/${pageId}`);
    toast('Deleted', 'success');
    renderEditorType(type);
  } catch (e) { toast(e.message, 'error'); }
}

async function renderContentEditor(type, pageId) {
  if (!isEditor()) return editorDenied();
  const app = $('#app');
  const info = ENTITY_TYPES[type];
  if (!info) { app.innerHTML = '<div class="empty"><h3>Unknown content type</h3></div>'; return; }
  const isNew = !pageId;
  app.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
  try {
    const descriptor = await loadContentSchema(type);
    let snapshot = { fields:{}, relations:{} };
    if (!isNew) snapshot = await api.get(`/api/content/${type}/${pageId}`);
    app.innerHTML = '';
    const v = el('div', { class:'view' });
    v.appendChild(crumbs([['', 'Archive'], ['editor', 'Editor Tools'],
      ['editor/' + type, info.plural], [null, isNew ? 'New' : 'Edit']]));
    v.appendChild(el('div', { class:'detail-header' },
      el('div', { class:'type' }, info.glyph + ' Editor'),
      el('h1', {}, (isNew ? 'New ' : 'Edit ') + info.single),
    ));
    const form = renderContentForm(descriptor, {
      mode: isNew ? 'create' : 'edit',
      values: snapshot.fields || {},
      relations: snapshot.relations || {},
    });
    const reasonIn = el('input', { type:'text', placeholder:'Optional note for the change history' });
    const save = el('button', { class:'btn btn-primary' }, isNew ? 'Create page' : 'Save changes');
    save.addEventListener('click', async () => {
      const changes = form.collect();
      if (!isNew && !Object.keys(changes.fields).length && !Object.keys(changes.relations).length) {
        toast('No changes to save', 'error'); return;
      }
      save.disabled = true;
      try {
        const reason = reasonIn.value.trim() || null;
        if (isNew) await api.post(`/api/content/${type}`, { changes, reason });
        else await api.put(`/api/content/${type}/${pageId}`, { changes, reason });
        toast(isNew ? 'Page created' : 'Changes saved', 'success');
        navigate('editor/' + type);
      } catch (e) { toast(e.message, 'error'); save.disabled = false; }
    });
    const body = el('div', { class:'detail-body' },
      form.node,
      el('div', { class:'field' }, el('label', {}, 'Change note'), reasonIn),
      el('div', { class:'modal-actions', style:{ marginTop:'1rem' }},
        el('button', { class:'btn btn-ghost', onclick:() => navigate('editor/' + type) }, 'Cancel'),
        save,
      ),
    );
    v.appendChild(body);
    app.appendChild(v);
  } catch (e) { app.innerHTML = ''; app.appendChild(errorBlock(e)); }
}
