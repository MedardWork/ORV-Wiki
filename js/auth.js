'use strict';

/* ============================================================
   USER AREA
============================================================ */
function renderUserArea() {
  const area = $('#userArea');
  area.innerHTML = '';
  if (State.user) {
    const initial = (State.user.username || State.user.email || '?')[0].toUpperCase();
    const btn = el('button', { class:'user-btn', onclick:openUserMenu },
      el('span', { class:'user-avatar' }, initial),
      el('span', { class:'hide-mobile' }, State.user.username),
    );
    area.appendChild(btn);
    $('#notifBtn').style.display = '';
    $('#menu-community').style.display = '';
    refreshUnread();
  } else {
    area.appendChild(el('button', { class:'btn btn-secondary btn-sm', onclick:() => openAuth('login') }, 'Sign in'));
    area.appendChild(el('button', { class:'btn btn-primary btn-sm', style:{ marginLeft:'.4rem' }, onclick:() => openAuth('register') }, 'Register'));
    $('#notifBtn').style.display = 'none';
    const cm = $('#menu-community'); if (cm) cm.style.display = 'none';
  }
  $('#chapterPillText').textContent = State.user ? 'Ch. ' + (State.user.currentChapter ?? 0) : 'Ch. —';
}

function openUserMenu() {
  openModal(close => {
    const u = State.user;
    return el('div', {},
      el('h2', {}, 'Reader Profile'),
      el('div', { class:'modal-sub' }, 'Your account in the Star Stream Archive'),
      el('div', { class:'side-card', style:{ background:'var(--space)', marginBottom:'1rem' }},
        el('div', { class:'row' }, el('span', { class:'k' }, 'Username'), el('span', { class:'v' }, u.username)),
        el('div', { class:'row' }, el('span', { class:'k' }, 'Email'), el('span', { class:'v' }, u.email)),
        el('div', { class:'row' }, el('span', { class:'k' }, 'Role'), el('span', { class:'v' }, u.role || 'reader')),
        el('div', { class:'row' }, el('span', { class:'k' }, 'Reading Chapter'), el('span', { class:'v' }, u.currentChapter ?? 0)),
      ),
      el('div', { style:{ display:'flex', flexDirection:'column', gap:'.4rem' }},
        el('button', { class:'btn btn-secondary', onclick:() => { close(); openChapterModal(); }}, '📖 Update reading chapter'),
        el('button', { class:'btn btn-secondary', onclick:() => { close(); openSettings(); }}, '⚙ Settings'),
        el('button', { class:'btn btn-danger', onclick:() => { close(); logout(); }}, 'Sign out'),
      ),
      el('p', { style:{ marginTop:'1rem', fontSize:'.78rem', color:'var(--text-3)' }},
        'Note: changing your reading chapter takes effect after you sign in again — the JWT token caches it.'
      ),
    );
  });
}

/* ============================================================
   AUTH
============================================================ */
function openAuth(mode='login') {
  openModal(close => {
    const root = el('div', {});
    const tabs = el('div', { class:'tabs' },
      el('button', { class:'tab'+(mode==='login'?' active':''), onclick:() => switchTab('login') }, 'Sign in'),
      el('button', { class:'tab'+(mode==='register'?' active':''), onclick:() => switchTab('register') }, 'Register'),
    );
    const form = el('div', {});
    function switchTab(m) { mode = m; renderForm(); $$('.tab', tabs).forEach((b,i) => b.classList.toggle('active', (i===0)===(m==='login'))); }
    function renderForm() {
      form.innerHTML = '';
      if (mode === 'login') {
        const idIn = el('input', { type:'text', placeholder:'username or email', autofocus:true });
        const pwIn = el('input', { type:'password', placeholder:'password' });
        const err  = el('div', { class:'field-error', style:{ display:'none' }});
        const submit = async () => {
          err.style.display = 'none';
          if (!idIn.value || !pwIn.value) { err.textContent = 'Both fields required'; err.style.display = 'block'; return; }
          try {
            const r = await api.post('/api/auth/login', { emailOrUsername: idIn.value.trim(), password: pwIn.value });
            applyAuth(r);
            close();
            toast('Welcome back, ' + State.user.username, 'success');
          } catch (e) { err.textContent = e.message; err.style.display = 'block'; }
        };
        form.appendChild(el('div', { class:'field' }, el('label', {}, 'Username or email'), idIn));
        form.appendChild(el('div', { class:'field' }, el('label', {}, 'Password'), pwIn));
        form.appendChild(err);
        form.appendChild(el('div', { class:'modal-actions' },
          el('button', { class:'btn btn-ghost', onclick:close }, 'Cancel'),
          el('button', { class:'btn btn-primary', onclick:submit }, 'Sign in'),
        ));
        pwIn.addEventListener('keydown', e => { if (e.key==='Enter') submit(); });
      } else {
        const emIn = el('input', { type:'email', placeholder:'reader@starstream.io', autofocus:true });
        const usIn = el('input', { type:'text', placeholder:'username' });
        const pwIn = el('input', { type:'password', placeholder:'password (≥ 12 chars recommended)' });
        const chIn = el('input', { type:'number', placeholder:'0', min:'0', value:'0' });
        const err = el('div', { class:'field-error', style:{ display:'none' }});
        const submit = async () => {
          err.style.display = 'none';
          try {
            const r = await api.post('/api/auth/register', {
              email: emIn.value.trim(), username: usIn.value.trim(),
              password: pwIn.value, currentChapter: parseInt(chIn.value) || 0,
            });
            applyAuth(r);
            close();
            toast('Welcome to the Archive, ' + State.user.username, 'success');
          } catch (e) { err.textContent = e.message; err.style.display = 'block'; }
        };
        form.appendChild(el('div', { class:'field' }, el('label', {}, 'Email'), emIn));
        form.appendChild(el('div', { class:'field' }, el('label', {}, 'Username'), usIn));
        form.appendChild(el('div', { class:'field' }, el('label', {}, 'Password'), pwIn));
        form.appendChild(el('div', { class:'field' }, el('label', {}, 'Current reading chapter'), chIn));
        form.appendChild(err);
        form.appendChild(el('div', { class:'modal-actions' },
          el('button', { class:'btn btn-ghost', onclick:close }, 'Cancel'),
          el('button', { class:'btn btn-primary', onclick:submit }, 'Create account'),
        ));
      }
    }
    root.appendChild(el('h2', {}, 'The Archive'));
    root.appendChild(el('div', { class:'modal-sub' }, 'Authenticated readers can comment, bookmark, and propose edits.'));
    root.appendChild(tabs);
    root.appendChild(form);
    renderForm();
    return root;
  });
}

function applyAuth(authResp) {
  State.token = authResp.accessToken || authResp.token;
  State.user  = authResp.user || decodeJwt(State.token);
  // Normalize fields possibly missing
  if (State.user) {
    State.user.username = State.user.username || State.user.unique_name || State.user.name || 'reader';
    State.user.currentChapter = State.user.currentChapter ?? Number(State.user.current_chapter) ?? 0;
    State.user.role = State.user.role || State.user.roles || 'reader';
  }
  renderUserArea();
}

function logout() {
  State.token = null;
  State.user = null;
  renderUserArea();
  navigate('');
  toast('Signed out');
}

/* ============================================================
   CHAPTER MODAL
============================================================ */
function openChapterModal() {
  if (!State.user) return openAuth('login');
  openModal(close => {
    const inp = el('input', { type:'number', min:'0', value: State.user.currentChapter ?? 0, autofocus:true });
    const submit = async () => {
      const n = parseInt(inp.value);
      if (isNaN(n) || n < 0) { toast('Enter a valid chapter number', 'error'); return; }
      try {
        // Backend re-issues the JWT with the new current_chapter claim — apply
        // it via applyAuth() so subsequent requests use the fresh token.
        const r = await api.patch('/api/users/me/current-chapter', { currentChapter: n });
        applyAuth(r);
        toast('Reading chapter set to ' + n, 'success');
        close();
        if (State.view) renderRoute();
      } catch (e) { toast(e.message, 'error'); }
    };
    return el('div', {},
      el('h2', {}, 'Reading Progress'),
      el('div', { class:'modal-sub' }, 'Set your current chapter to control which entries and comments are revealed.'),
      el('div', { class:'field' }, el('label', {}, 'Current chapter'), inp),
      el('p', { style:{ fontSize:'.8rem', color:'var(--text-3)', marginTop:'.4rem' }},
        'The Star Stream is patient. New entries and inline reveals appear as your reading advances.'),
      el('div', { class:'modal-actions' },
        el('button', { class:'btn btn-ghost', onclick:close }, 'Cancel'),
        el('button', { class:'btn btn-primary', onclick:submit }, 'Update'),
      ),
    );
  });
}

/* ============================================================
   SETTINGS
============================================================ */
function openSettings() {
  openModal(close => {
    const apiIn = el('input', { type:'text', value: State.apiBase, placeholder:'https://localhost:7138' });
    const spoilerIn = el('input', { type:'checkbox' });
    spoilerIn.checked = State.ignoreSpoilers;
    return el('div', {},
      el('h2', {}, 'Settings'),
      el('div', { class:'modal-sub' }, 'Configure where the frontend speaks to the backend.'),
      el('div', { class:'field' },
        el('label', {}, 'API base URL'),
        apiIn,
        el('div', { style:{ fontSize:'.78rem', color:'var(--text-3)', marginTop:'.4rem' }},
          'Backend defaults: ', el('code', {}, 'https://localhost:7138'), ' or ', el('code', {}, 'http://localhost:5044'),
        ),
      ),
      el('div', { class:'field' },
        el('label', { style:{ display:'flex', alignItems:'center', gap:'.5rem', cursor:'pointer' }},
          spoilerIn,
          el('span', {}, 'Show every page (disable spoiler gate)'),
        ),
        el('div', { style:{ fontSize:'.78rem', color:'var(--text-3)', marginTop:'.4rem' }},
          'When enabled, all wiki entries are visible regardless of your reading chapter. ',
          'Inline ', el('code', {}, '[spoiler ch=N]'), ' blocks are still shown blacked-out per-section.',
        ),
      ),
      el('div', { class:'modal-actions' },
        el('button', { class:'btn btn-ghost', onclick:close }, 'Cancel'),
        el('button', { class:'btn btn-primary', onclick:() => {
          State.apiBase = apiIn.value.trim().replace(/\/$/,'');
          State.ignoreSpoilers = spoilerIn.checked;
          savePrefs();
          toast('Settings saved', 'success');
          close();
          renderRoute();
        }}, 'Save'),
      ),
    );
  });
}
