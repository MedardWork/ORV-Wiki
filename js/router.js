'use strict';

/* ============================================================
   ROUTER
============================================================ */
function navigate(path) {
  location.hash = '#/' + path.replace(/^\/+/, '');
}
function parseRoute() {
  const h = location.hash.replace(/^#\/?/, '');
  const parts = h.split('/').filter(Boolean);
  return parts;
}
function renderRoute() {
  const p = parseRoute();
  const app = $('#app');
  app.innerHTML = '<div class="loader"><div class="loader-spinner"></div></div>';
  if (p.length === 0) return renderHome();
  const head = p[0];
  if (head === 'category')   return renderCategory(p[1], p[2]);
  if (head === 'page')       return renderPage(p[1]);
  if (head === 'character')  return renderCharacter(p[1]);
  if (head === 'tag')        return renderTag(p[1], p[2]);
  if (head === 'timeline')   return renderTimeline();
  if (head === 'bookmarks')  return renderBookmarks();
  if (head === 'suggestions')return renderMySuggestions();
  if (head === 'queue')      return renderQueue();
  if (head === 'editor') {
    if (!p[1])          return renderEditorHome();
    if (!p[2])          return renderEditorType(p[1]);
    if (p[2] === 'new') return renderContentEditor(p[1], null);
    return renderContentEditor(p[1], p[2]);
  }
  app.innerHTML = '<div class="empty"><div class="glyph">✦</div><h3>Lost in the Star Stream</h3><p>That route does not exist.</p></div>';
}
addEventListener('hashchange', renderRoute);
