/**
 * upload.mjs
 *
 * For every PNG in ./output/ :
 *   1. Derives metadata from the profile code (Family / Series / Category / System / SortOrder)
 *   2. Checks if the code already exists in the DB; if so goes straight to image upload
 *   3. POSTs to /api/profile-diagrams  →  gets the new id
 *   4. POSTs the PNG to /api/profile-diagrams/{id}/upload-image
 *
 * Run:   node upload.mjs
 * Opts:  CONCURRENCY=4  API_BASE=https://localhost:5001  SKIP_EXISTING=1
 */

// ──  dev-only: trust the self-signed dev cert  ───────────────────────────────
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

import { readdirSync, readFileSync } from 'fs';
import { join, basename } from 'path';

const OUTPUT_DIR  = join(import.meta.dirname, 'output');
const API_BASE    = process.env.API_BASE ?? 'https://localhost:5001';
const CONCURRENCY = Number(process.env.CONCURRENCY ?? 3);
const SKIP_IMG_IF_EXISTS = process.env.SKIP_EXISTING !== '0'; // default true

// ─── Metadata derivation ─────────────────────────────────────────────────────

const FAMILY_MAP = {
  AS: 'Aluminium System',
  AL: 'Aluminium Large',
  AC: 'Aluminium Casement',
  AA: 'Accessories',
};

const SERIES_SYSTEM_MAP = {
  '23': '23mm Narrow Sliding',
  '28': '28mm Slim Sliding',
  '30': '30mm Casement',
  '32': '32mm Economy Sliding',
  '35': '35mm Standard Sliding',
  '39': '39mm Casement',
  '44': '44mm Luxury Sliding',
  '45': '45mm Casement',
  '50': '50mm Super Luxury Sliding',
  '56': '56mm Large Format Sliding',
  '62': '62mm Extra Large Sliding',
  '65': '65mm Large Casement',
  '00': 'Common Accessories',
};

const CATEGORY_LABEL_MAP = {
  PF: 'Outer Frame',
  PS: 'Sash',
  PE: 'Profile End',
  PB: 'Bead',
  ME: 'Mullion',
  PT: 'Transom',
  PC: 'Profile Cap',
  RT: 'Rubber Track',
  PY: 'Pulley',
  PO: 'Profile Other',
  HE: 'Hardware',
  MM: 'Misc',
  PD: 'Profile Damper',
  PL: 'Profile Lock',
  PH: 'Profile Hook',
  PX: 'Profile Extension',
  PB2: 'Bead 2',
  MS: 'Mesh',
};

// Sort within each row: family→series→category→sequence
function sortKey(code) {
  const m = code.match(/^([A-Z]{2})(\d{2,3})([A-Z]{2})(\d{2,3})$/);
  if (!m) return code;
  const [, fam, ser, cat, seq] = m;
  return `${fam}${ser.padStart(3,'0')}${cat}${seq.padStart(3,'0')}`;
}

function metaFromCode(code) {
  const m = code.match(/^([A-Z]{2})(\d{2,3})([A-Z]{2})(\d{2,3})$/);
  if (!m) return { family: '', series: '', category: '', categoryLabel: '', system: '', sortOrder: 0 };
  const [, fam, ser, cat, seq] = m;
  return {
    family:        FAMILY_MAP[fam]                 ?? fam,
    series:        ser,
    category:      cat,
    categoryLabel: CATEGORY_LABEL_MAP[cat]         ?? cat,
    system:        SERIES_SYSTEM_MAP[ser]           ?? `${ser}mm Series`,
    sortOrder:     0,
  };
}

// ─── HTTP helpers ─────────────────────────────────────────────────────────────

async function apiGet(path) {
  const r = await fetch(`${API_BASE}${path}`);
  return { status: r.status, data: await r.json() };
}

async function apiPost(path, body) {
  const r = await fetch(`${API_BASE}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  return { status: r.status, data: await r.json() };
}

async function apiUploadImage(id, pngBytes, code) {
  const blob = new Blob([pngBytes], { type: 'image/png' });
  const form = new FormData();
  form.append('file', blob, `${code}.png`);
  const r = await fetch(`${API_BASE}/api/profile-diagrams/${id}/upload-image`, {
    method: 'POST',
    body: form,
  });
  return { status: r.status, data: await r.json() };
}

// ─── Load existing codes from DB (to skip create step) ────────────────────────

async function loadExistingCodes() {
  const existing = new Map(); // code → { id, hasImage }
  let page = 1;
  const pageSize = 200;
  while (true) {
    const { data } = await apiGet(`/api/profile-diagrams?pageSize=${pageSize}&page=${page}&sortBy=code`);
    const items = data?.data?.items ?? [];
    for (const item of items) {
      existing.set(item.code.toUpperCase(), { id: item.id, hasImage: !!item.imageUrl });
    }
    if (items.length < pageSize) break;
    page++;
  }
  return existing;
}

// ─── Worker pool ──────────────────────────────────────────────────────────────

async function processOne(code, pngPath, existingMap, stats) {
  const pngBytes = readFileSync(pngPath);
  const meta = metaFromCode(code);
  const existing = existingMap.get(code.toUpperCase());

  let id;

  if (existing) {
    id = existing.id;
    if (existing.hasImage && SKIP_IMG_IF_EXISTS) {
      console.log(`  ⏭  ${code}  (record + image already exist)`);
      stats.skipped++;
      return;
    }
    console.log(`  ↩  ${code}  (record exists id=${id}, re-uploading image…)`);
  } else {
    // Create the record
    const { status, data: createData } = await apiPost('/api/profile-diagrams', {
      code,
      family:        meta.family,
      series:        meta.series,
      category:      meta.category,
      categoryLabel: meta.categoryLabel,
      system:        meta.system,
      sortOrder:     meta.sortOrder,
      notes:         null,
    });

    if (status === 201) {
      id = createData?.data?.id;
      console.log(`  ✚  ${code}  created id=${id}`);
    } else if (status === 409) {
      // Race condition or stale cache — try to fetch the id
      const { data: search } = await apiGet(`/api/profile-diagrams?codes=${encodeURIComponent(code)}`);
      id = search?.data?.items?.[0]?.id;
      console.log(`  ↩  ${code}  conflict — found id=${id}`);
    } else {
      console.error(`  ✗  ${code}  CREATE failed (HTTP ${status}): ${JSON.stringify(createData).slice(0, 120)}`);
      stats.errors++;
      return;
    }
  }

  if (!id) {
    console.error(`  ✗  ${code}  could not resolve id — skipping`);
    stats.errors++;
    return;
  }

  // Upload image
  const { status: upStatus, data: upData } = await apiUploadImage(id, pngBytes, code);
  if (upStatus === 200) {
    const url = upData?.data ?? '';
    console.log(`  ✓  ${code}  image → ${url.split('/').pop()}`);
    stats.uploaded++;
  } else {
    console.error(`  ✗  ${code}  UPLOAD failed (HTTP ${upStatus}): ${JSON.stringify(upData).slice(0, 120)}`);
    stats.errors++;
  }
}

// ─── Main ─────────────────────────────────────────────────────────────────────

const files = readdirSync(OUTPUT_DIR)
  .filter(f => f.endsWith('.png'))
  .sort((a, b) => sortKey(a.replace('.png','')).localeCompare(sortKey(b.replace('.png',''))));

console.log(`Found ${files.length} PNG files in output/`);

console.log('Loading existing DB records …');
const existingMap = await loadExistingCodes();
console.log(`  → ${existingMap.size} codes already in DB\n`);

const stats = { uploaded: 0, skipped: 0, errors: 0 };
const queue = [...files];

// Process in batches of CONCURRENCY
async function runWorker() {
  while (queue.length > 0) {
    const file = queue.shift();
    if (!file) break;
    const code = basename(file, '.png');
    const pngPath = join(OUTPUT_DIR, file);
    await processOne(code, pngPath, existingMap, stats);
  }
}

const workers = Array.from({ length: CONCURRENCY }, runWorker);
await Promise.all(workers);

console.log(`\n${'─'.repeat(50)}`);
console.log(`Uploaded : ${stats.uploaded}`);
console.log(`Skipped  : ${stats.skipped}`);
console.log(`Errors   : ${stats.errors}`);
console.log(`Total    : ${files.length}`);
