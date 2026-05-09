'use strict';

/* ============================================================
   STARFIELD
============================================================ */
function startStarfield() {
  const c = $('#starfield');
  const ctx = c.getContext('2d');
  let stars = [];
  function resize() {
    c.width = innerWidth * devicePixelRatio;
    c.height = innerHeight * devicePixelRatio;
    c.style.width = innerWidth+'px';
    c.style.height = innerHeight+'px';
    ctx.scale(devicePixelRatio, devicePixelRatio);
    const n = Math.floor(innerWidth*innerHeight/2200);
    stars = Array.from({length:n}, () => ({
      x: Math.random()*innerWidth,
      y: Math.random()*innerHeight,
      r: Math.random()*1.2 + 0.2,
      tw: Math.random()*Math.PI*2,
      sp: Math.random()*0.6 + 0.2,
      hue: Math.random() < 0.15 ? 'gold' : 'white',
    }));
  }
  let t = 0;
  function frame() {
    ctx.clearRect(0,0,innerWidth,innerHeight);
    for (const s of stars) {
      const a = (Math.sin(t*s.sp + s.tw) + 1) / 2 * 0.7 + 0.15;
      ctx.beginPath();
      ctx.arc(s.x, s.y, s.r, 0, Math.PI*2);
      if (s.hue === 'gold') {
        ctx.fillStyle = `rgba(240,198,116,${a})`;
        ctx.shadowColor = 'rgba(240,198,116,.7)';
        ctx.shadowBlur = 6;
      } else {
        ctx.fillStyle = `rgba(220,220,240,${a})`;
        ctx.shadowBlur = 0;
      }
      ctx.fill();
    }
    ctx.shadowBlur = 0;
    t += 0.012;
    requestAnimationFrame(frame);
  }
  resize();
  addEventListener('resize', resize);
  frame();
}

/* ============================================================
   HEADER MENUS
============================================================ */
function buildMenubar() {
  const bar = $('#menubar');
  bar.innerHTML = '';
  for (const g of MENU_GROUPS) {
    const menu = el('div', { class:'menu' });
    const btn  = el('button', { class:'menu-btn', onclick:e => toggleMenu(menu) }, g.label, el('span', { class:'caret' }, '▾'));
    const panel= el('div', { class:'menu-panel' });
    panel.appendChild(el('div', { class:'menu-label' }, g.label));
    for (const t of g.types) {
      const m = ENTITY_TYPES[t];
      panel.appendChild(el('button', {
        class:'menu-item',
        onclick:() => { closeAllMenus(); navigate('category/'+t); },
      }, el('span', { class:'icon' }, m.glyph), el('span', {}, m.plural)));
    }
    menu.appendChild(btn);
    menu.appendChild(panel);
    bar.appendChild(menu);
  }
  // Community menu (only when logged in)
  const community = el('div', { class:'menu', id:'menu-community', style:{ display: State.user ? '' : 'none' }});
  const cBtn = el('button', { class:'menu-btn', onclick:() => toggleMenu(community) }, 'Community', el('span', { class:'caret' }, '▾'));
  const cPanel = el('div', { class:'menu-panel' });
  cPanel.appendChild(el('div', { class:'menu-label' }, 'My Activity'));
  cPanel.appendChild(el('button', { class:'menu-item', onclick:() => { closeAllMenus(); navigate('bookmarks'); }}, el('span', { class:'icon' }, '★'), 'My Bookmarks'));
  cPanel.appendChild(el('button', { class:'menu-item', onclick:() => { closeAllMenus(); navigate('suggestions'); }}, el('span', { class:'icon' }, '✎'), 'My Edit Suggestions'));
  // Editor-only: the queue endpoint requires Editor or Admin, so readers
  // would just hit a 403 if they clicked it.
  if (State.user && (State.user.role === 'editor' || State.user.role === 'admin')) {
    cPanel.appendChild(el('div', { class:'menu-divider' }));
    cPanel.appendChild(el('div', { class:'menu-label' }, 'Review'));
    cPanel.appendChild(el('button', { class:'menu-item', onclick:() => { closeAllMenus(); navigate('queue'); }}, el('span', { class:'icon' }, '⚖'), 'Suggestion Queue'));
  }
  community.appendChild(cBtn);
  community.appendChild(cPanel);
  bar.appendChild(community);

  // Timeline link
  bar.appendChild(el('button', {
    class:'menu-btn',
    onclick:() => { closeAllMenus(); navigate('timeline'); },
  }, '∞ Timeline'));
}
function toggleMenu(menu) {
  const open = menu.classList.contains('open');
  closeAllMenus();
  if (!open) menu.classList.add('open');
}
function closeAllMenus() { $$('.menu.open').forEach(m => m.classList.remove('open')); }
document.addEventListener('click', e => { if (!e.target.closest('.menu')) closeAllMenus(); });
