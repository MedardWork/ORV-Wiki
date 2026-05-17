'use strict';

/* ============================================================
   COMMENTS
============================================================ */
function commentsSection(p) {
  const wrap = el('div', { class:'detail-body', id:'comments-block' });
  wrap.appendChild(el('h2', {}, '✦ Reader Discourse'));
  wrap.appendChild(el('p', { style:{ color:'var(--text-3)', fontSize:'.85rem', marginTop:'-.4rem', marginBottom:'1rem' }},
    'Only comments posted by readers at or below your current chapter are shown.'));

  if (State.user) {
    const ta = el('textarea', { placeholder:'Add to the discourse… (be mindful of spoilers — your comment will only show to readers who have reached your current chapter)' });
    const submit = async () => {
      if (!ta.value.trim()) return;
      try {
        await api.post('/api/comments', { pageId: p.id, body: ta.value.trim() });
        ta.value = '';
        loadComments();
      } catch (e) { toast(e.message, 'error'); }
    };
    wrap.appendChild(el('div', { class:'comment-form' },
      ta,
      el('div', { class:'comment-form-actions' },
        el('button', { class:'btn btn-primary btn-sm', onclick:submit }, 'Post'),
      ),
    ));
  } else {
    wrap.appendChild(el('div', { class:'side-card', style:{ background:'var(--space)', textAlign:'center' }},
      el('p', { style:{ marginBottom:'.6rem' }}, 'Sign in to participate in the discourse.'),
      el('button', { class:'btn btn-primary btn-sm', onclick:() => openAuth('login') }, 'Sign in')));
  }

  const list = el('div', { class:'comment-thread' });
  wrap.appendChild(list);

  async function loadComments() {
    list.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
    try {
      const r = await api.get('/api/comments?pageId=' + p.id);
      const items = r?.items || r || [];
      list.innerHTML = '';
      if (!items.length) {
        list.appendChild(el('div', { class:'empty' },
          el('div', { class:'glyph' }, '✧'),
          el('p', {}, 'The discourse is silent. Be the first.'),
        ));
        return;
      }
      // Build tree
      const byId = {}; items.forEach(c => { c._kids = []; byId[c.id] = c; });
      const roots = [];
      items.forEach(c => {
        const pid = c.parentCommentId ?? c.parent_comment_id;
        if (pid && byId[pid]) byId[pid]._kids.push(c);
        else roots.push(c);
      });
      roots.forEach(c => list.appendChild(commentNode(c, p)));
    } catch (e) {
      list.innerHTML = '';
      list.appendChild(errorBlock(e));
    }
  }
  loadComments();
  wrap.dataset.refresh = 'comments';
  return wrap;
}

function commentNode(c, p) {
  const node = el('div', { class:'comment' });
  const ch = c.chapterAtPost ?? c.chapter_at_post;
  const deleted = c.isDeleted ?? c.is_deleted;
  node.appendChild(el('div', { class:'comment-head' },
    el('span', { class:'author' }, c.username || (deleted ? '[deleted]' : 'reader')),
    ch !== undefined ? el('span', { class:'ch-tag' }, 'Ch. '+ch) : null,
    el('span', { class:'ago' }, fmtDate(c.createdAt || c.created_at)),
  ));
  node.appendChild(el('div', { class:'comment-body'+(deleted ? ' deleted' : '') }, deleted ? '[comment removed]' : (c.body || '')));

  if (!deleted) {
    const acts = el('div', { class:'comment-actions' });
    // Reactions
    const reactions = c.reactions || c.Reactions || [];
    for (const rx of REACTIONS) {
      const summary = reactions.find(s => (s.type || s.Type) === rx.type) || { count: 0, byMe: false };
      const myReacted = summary.byMe ?? summary.ByMe ?? false;
      const btn = el('button', {
        class:'react-btn'+(myReacted ? ' active' : ''),
        onclick:async () => {
          if (!State.user) return openAuth('login');
          try {
            await api.post(`/api/comments/${c.id}/reactions/${rx.type}`);
            // Optimistic refresh of this comment block
            const block = node.closest('[data-refresh="comments"]');
            if (block) {
              // re-fetch from API
              const list = block.querySelector('.comment-thread');
              const evt = new Event('refresh');
              // simpler — reload entire comments by triggering parent
              const allComments = await api.get('/api/comments?pageId='+p.id);
              const items = allComments?.items || allComments || [];
              const updated = items.find(x => x.id === c.id);
              if (updated) {
                const newNode = commentNode({ ...updated, _kids: c._kids }, p);
                node.replaceWith(newNode);
              }
            }
          } catch (e) { toast(e.message, 'error'); }
        },
      }, rx.emoji, summary.count > 0 ? el('span', { class:'count' }, summary.count) : null);
      acts.appendChild(btn);
    }
    // Reply
    if (State.user) {
      acts.appendChild(el('button', { class:'react-btn', onclick:() => openReply(c, p, node) }, '↳ Reply'));
      const isMine = State.user.id == c.userId || State.user.username === c.username;
      if (isMine || State.user.role === 'editor' || State.user.role === 'admin') {
        acts.appendChild(el('button', { class:'react-btn', onclick:async () => {
          if (!confirm('Delete this comment?')) return;
          try { await api.del('/api/comments/'+c.id); node.querySelector('.comment-body').textContent = '[comment removed]'; node.querySelector('.comment-body').classList.add('deleted'); acts.remove(); }
          catch (e) { toast(e.message, 'error'); }
        }}, '✕ Delete'));
      }
    }
    node.appendChild(acts);
  }

  if (c._kids && c._kids.length) {
    const kids = el('div', { class:'comment-children' });
    c._kids.forEach(k => kids.appendChild(commentNode(k, p)));
    node.appendChild(kids);
  }
  return node;
}

function openReply(parent, page, parentNode) {
  const ta = el('textarea', { placeholder:'Reply…', autofocus:true });
  const form = el('div', { class:'comment-form', style:{ marginTop:'.6rem' }},
    ta,
    el('div', { class:'comment-form-actions' },
      el('button', { class:'btn btn-ghost btn-sm', onclick:() => form.remove() }, 'Cancel'),
      el('button', { class:'btn btn-primary btn-sm', onclick:async () => {
        if (!ta.value.trim()) return;
        try {
          await api.post('/api/comments', { pageId: page.id, body: ta.value.trim(), parentCommentId: parent.id });
          form.remove();
          // Reload thread
          const block = parentNode.closest('[data-refresh="comments"]');
          if (block) {
            const all = await api.get('/api/comments?pageId='+page.id);
            const items = all?.items || all || [];
            const list = block.querySelector('.comment-thread');
            list.innerHTML = '';
            const byId = {}; items.forEach(c => { c._kids = []; byId[c.id] = c; });
            const roots = [];
            items.forEach(c => { const pid = c.parentCommentId ?? c.parent_comment_id; if (pid && byId[pid]) byId[pid]._kids.push(c); else roots.push(c); });
            roots.forEach(c => list.appendChild(commentNode(c, page)));
          }
        } catch (e) { toast(e.message, 'error'); }
      }}, 'Post reply'),
    ),
  );
  parentNode.appendChild(form);
}

/* ============================================================
   EDIT SUGGEST — openSuggest / openSuggestNew now live in
   js/editor.js: schema-driven forms covering every content type.
============================================================ */
