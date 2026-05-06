'use strict';

/* ============================================================
   GLOBAL DELEGATION FOR data-route
============================================================ */
document.addEventListener('click', e => {
  const t = e.target.closest('[data-route]');
  if (t) { e.preventDefault(); navigate(t.dataset.route); }
});

/* ============================================================
   INIT
============================================================ */
function init() {
  startStarfield();
  buildMenubar();
  renderUserArea();
  if (!location.hash) location.hash = '#/';
  renderRoute();
}
init();
