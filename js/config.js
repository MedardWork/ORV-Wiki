'use strict';

// Per-deployment configuration. Loaded BEFORE core.js so the value is
// available when State is initialized.
//
// Edit this single line when the API moves. core.js falls back to its own
// hostname-aware defaults if this is left empty / undefined.
window.ORV_API_BASE = 'https://orv-api-production.up.railway.app';
