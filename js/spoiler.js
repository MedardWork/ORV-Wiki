'use strict';

/* ============================================================
   SPOILER RENDERING

   The backend serialises any narrative field that may carry inline
   `[spoiler ch=N]…[/spoiler]` markup as a `RenderedContent`:

     { segments: [
         { type:'text',    content:'…',   revealChapter:null },
         { type:'spoiler', content:'…',   revealChapter:N    },  // revealed
         { type:'spoiler', content:null,  revealChapter:N    },  // hidden — server stripped
       ],
       hasHiddenContent: true|false }

   Helpers:
     plainTextOf(content)   — extract a flattened string (hidden spoilers
                              become "[ch.N+ spoiler]"); good for tooltips,
                              search filters, suggestion textareas.
     hasHidden(content)     — true iff any segment is a hidden spoiler.
     hasContent(content)    — true iff there is text OR a hidden-spoiler chip
                              worth rendering. Plain strings count when non-
                              empty.
     renderRendered(c,p)    — append rich nodes (text + spoiler chips) to p.
                              Each `\n` becomes a <br>; `\n\n` therefore
                              renders as two <br> which is enough vertical
                              spacing for biography-style fields.
     renderedNode(c,tag,ps) — convenience: build a single element with
                              rendered children. Equivalent to
                              `const e=el(tag,ps); renderRendered(c,e); return e;`
     spoilerBanner()        — the "this field has hidden content" notice.

   The functions accept three input shapes for backwards compatibility:
     1. A RenderedContent object (current backend output).
     2. A plain string (for legacy fields not yet wrapped).
     3. null/undefined (renders nothing). */
function _segs(content) {
  if (content == null) return null;
  if (typeof content === 'string') return null;
  return content.segments || content.Segments || null;
}
function plainTextOf(content) {
  if (content == null) return '';
  if (typeof content === 'string') return content;
  const segs = _segs(content);
  if (!segs) return '';
  return segs.map(s => {
    const t = (s.type || s.Type || '').toLowerCase();
    const text = s.content ?? s.Content;
    const ch   = s.revealChapter ?? s.RevealChapter;
    if (t === 'spoiler' && (text === null || text === undefined)) return `[ch.${ch}+ spoiler]`;
    return text || '';
  }).join('');
}
function hasHidden(content) {
  if (!content || typeof content === 'string') return false;
  const flag = content.hasHiddenContent ?? content.HasHiddenContent;
  if (typeof flag === 'boolean') return flag;
  const segs = _segs(content);
  if (!segs) return false;
  return segs.some(s => (s.type || s.Type || '').toLowerCase() === 'spoiler'
    && (s.content ?? s.Content) == null);
}
// True when a value is a non-empty string OR a RenderedContent with at least
// one segment carrying text or a hidden-spoiler chip.
function hasContent(v) {
  if (v == null) return false;
  if (typeof v === 'string') return v.length > 0;
  const segs = _segs(v);
  if (!segs) return false;
  return segs.some(s => {
    const t = (s.type || s.Type || '').toLowerCase();
    const text = s.content ?? s.Content;
    return (t === 'spoiler') || (text != null && String(text).length > 0);
  });
}
function _appendText(parent, text) {
  // Each '\n' → <br>. Splitting on /\n/ keeps empty strings between
  // consecutive newlines so '\n\n' inserts two <br> for paragraph spacing.
  const lines = String(text ?? '').split(/\n/);
  lines.forEach((ln, i) => {
    if (i > 0) parent.appendChild(el('br'));
    if (ln.length) parent.appendChild(document.createTextNode(ln));
  });
}
function renderRendered(content, parent) {
  if (content == null) return;
  if (typeof content === 'string') { _appendText(parent, content); return; }
  const segs = _segs(content);
  if (!segs || !segs.length) return;
  for (const s of segs) {
    const t = (s.type || s.Type || '').toLowerCase();
    const text = s.content ?? s.Content;
    const ch = s.revealChapter ?? s.RevealChapter;
    if (t === 'spoiler' && (text === null || text === undefined || text === '')) {
      // Hidden — server stripped the content. Show a clickable chip.
      parent.appendChild(el('span', {
        class:'spoiler hidden',
        title:`Hidden until Ch. ${ch}. Update your reading chapter to reveal.`,
      }, `Ch. ${ch}+ spoiler`));
    } else if (t === 'spoiler') {
      // Revealed — text is visible. Tag with a small "ch.N" badge so the
      // reader can see what chapter the reveal lands at.
      const span = el('span', {
        class:'spoiler revealed',
        title:`Reveal at Ch. ${ch}`,
        'data-ch': String(ch ?? ''),
      });
      _appendText(span, text);
      parent.appendChild(span);
    } else {
      _appendText(parent, text);
    }
  }
}
function renderedNode(content, tag='p', props={}) {
  const node = el(tag, props);
  renderRendered(content, node);
  return node;
}
function spoilerBanner(label='This field contains revelations beyond your current reading chapter. Continue reading to unlock them.') {
  return el('div', { class:'spoiler-banner' }, label);
}
