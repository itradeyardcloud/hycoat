/**
 * extract.mjs  (v2 — content-aware crop, high-DPI)
 *
 * Reads the Fenesta "Master Profile Chart of All Aluminum System" PDF,
 * locates every profile-code label, measures the actual graphic/dimension-text
 * extent of each profile (not just the code-label width), and renders
 * a tight-but-complete crop at 432 DPI (SCALE=6).
 *
 * Usage:
 *   node extract.mjs                    # render all → ./output/
 *   SCALE=8 node extract.mjs            # higher DPI (576 DPI)
 *   FORCE=1 node extract.mjs            # overwrite existing PNGs
 *   DRY_RUN=1 node extract.mjs          # just print crop rects
 */

import mupdf from 'mupdf';
import { readFileSync, mkdirSync, writeFileSync, existsSync, rmSync } from 'fs';
import { join } from 'path';

// ── config ────────────────────────────────────────────────────────────────────
const PDF_PATH = join(
  'D:/IProject/ToBeDone',
  '549-R1_Master Profile Chart of All Aluminium System-R1-(20.01.26) (1).pdf'
);
const OUTPUT_DIR = join(import.meta.dirname, 'output');
const SCALE   = Number(process.env.SCALE   ?? 6);    // 6 × 72 ≈ 432 DPI
const DRY_RUN = process.env.DRY_RUN === '1';
const FORCE   = process.env.FORCE   === '1';         // re-render even if file exists

// ── profile code pattern ──────────────────────────────────────────────────────
// Covers: AS35PE03, AL56PF01, AA00PE04, AC39PB01, AC65PF01, AA00HE02 …
const CODE_RE = /^[A-Z]{2}\d{2,3}[A-Z]{2}\d{2,3}$/;

// Extra padding added outside the measured content boundary (pts)
const SIDE_PAD   = 12;  // horizontal — generous so dimension ticks are never cut
const TOP_PAD    = 8;   // above top-most dimension text in the band
const BOTTOM_PAD = 4;   // below code-label underline

function clamp(v, lo, hi) { return Math.max(lo, Math.min(hi, v)); }

// ── render a crop region of the DisplayList ───────────────────────────────────
function renderCrop(dl, [x0, y0, x1, y1], scale) {
  const px0 = Math.floor(x0 * scale);
  const py0 = Math.floor(y0 * scale);
  const px1 = Math.ceil(x1 * scale);
  const py1 = Math.ceil(y1 * scale);

  const pixmap = new mupdf.Pixmap(mupdf.ColorSpace.DeviceRGB, [px0, py0, px1, py1], false);
  pixmap.clear(255);                                   // white background
  const matrix = [scale, 0, 0, scale, 0, 0];
  const device = new mupdf.DrawDevice(matrix, pixmap);
  dl.run(device, mupdf.Matrix.identity);
  device.close();
  const png = pixmap.asPNG();
  pixmap.destroy();
  return png;
}

// ── main ──────────────────────────────────────────────────────────────────────
console.log('Loading PDF …');
const pdfBuf = readFileSync(PDF_PATH);
const doc    = mupdf.Document.openDocument(pdfBuf, 'application/pdf');
const page   = doc.loadPage(0);
const [, , pageW, pageH] = page.getBounds();
console.log(`Page size: ${(pageW / 2.835).toFixed(1)} × ${(pageH / 2.835).toFixed(1)} mm   (${pageW.toFixed(0)} × ${pageH.toFixed(0)} pts)`);

// ── text extraction ───────────────────────────────────────────────────────────
console.log('Extracting text …');
const dl    = page.toDisplayList();
const stext = dl.toStructuredText('preserve-whitespace,preserve-spans');
const stObj = JSON.parse(stext.asJSON());

// Collect ALL text items: code labels + dimension annotations
const allText   = [];   // every text item on the page
const codeItems = [];   // only profile code labels

for (const block of stObj.blocks) {
  if (!block.lines) continue;
  for (const line of block.lines) {
    const text = (line.text || '').trim();
    if (!text) continue;
    const b = line.bbox;
    const item = { text, x: b.x, y: b.y, w: b.w, h: b.h };
    allText.push(item);
    if (CODE_RE.test(text)) codeItems.push(item);
  }
}
console.log(`All text items: ${allText.length}   Profile codes: ${codeItems.length}`);

// ── group code labels into rows (Y-proximity ±8 pts) ─────────────────────────
codeItems.sort((a, b) => a.y - b.y || a.x - b.x);

const rows = [];
let cur = [codeItems[0]];
for (let i = 1; i < codeItems.length; i++) {
  if (Math.abs(codeItems[i].y - codeItems[i - 1].y) < 8) {
    cur.push(codeItems[i]);
  } else {
    rows.push(cur);
    cur = [codeItems[i]];
  }
}
rows.push(cur);
console.log(`Grouped into ${rows.length} rows.\n`);

// Pre-compute per-row label-bottom (= y + h for the lowest label in the row)
const rowLabelBottom = rows.map(r =>
  Math.max(...r.map(c => c.y + c.h))
);

// ── build content-aware crop rects ───────────────────────────────────────────
if (!DRY_RUN) {
  mkdirSync(OUTPUT_DIR, { recursive: true });
  if (FORCE) {
    // Wipe existing output so everything is re-rendered
    try { rmSync(OUTPUT_DIR, { recursive: true, force: true }); } catch (_) {}
    mkdirSync(OUTPUT_DIR, { recursive: true });
  }
}

let saved = 0, skipped = 0;

for (let ri = 0; ri < rows.length; ri++) {
  const row = [...rows[ri]].sort((a, b) => a.x - b.x);

  // ── Y band for this row ─────────────────────────────────────────────────────
  // Top of band = bottom of previous row's labels + small gap
  const bandY0 = ri === 0
    ? 0
    : rowLabelBottom[ri - 1] + 2;
  // Bottom of band = bottom of this row's labels + small margin
  const bandY1 = rowLabelBottom[ri] + BOTTOM_PAD;

  // Collect ALL text items that fall inside this row's Y band
  // (includes dimension numbers, series labels, etc.)
  const bandText = allText.filter(t =>
    t.y >= bandY0 - 2 && (t.y + t.h) <= bandY1 + 4
  );

  for (let ci = 0; ci < row.length; ci++) {
    const item    = row[ci];
    const prevCode = row[ci - 1];
    const nextCode = row[ci + 1];

    // ── rough X boundary between this code and its neighbours ────────────────
    // This separates which text items "belong" to which profile.
    // Cap at MAX_HALF_WIDTH from the code-label centre so the last/first item
    // in a row cannot grab text from remote parts of the sheet.
    const MAX_HALF_WIDTH = 110;  // pts — ~39 mm; wider than any profile in this chart
    const codeCenter = item.x + item.w / 2;
    const xSepLeft = prevCode
      ? Math.max((prevCode.x + prevCode.w + item.x) / 2, codeCenter - MAX_HALF_WIDTH)
      : Math.max(codeCenter - MAX_HALF_WIDTH, 0);
    const xSepRight = nextCode
      ? Math.min((item.x + item.w + nextCode.x) / 2, codeCenter + MAX_HALF_WIDTH)
      : Math.min(codeCenter + MAX_HALF_WIDTH, pageW);

    // ── find all text in band whose X centre is within 60 pts of this code ──
    // Using a radius rather than the hard midpoint-separator so dimension
    // annotations that sit just outside the midpoint are still captured.
    // The MAX_HALF_WIDTH cap prevents runaway capture toward remote page content.
    const CAPTURE_RADIUS = 60;  // pts from code centre
    const captureLeft  = Math.max(codeCenter - CAPTURE_RADIUS, xSepLeft  - SIDE_PAD);
    const captureRight = Math.min(codeCenter + CAPTURE_RADIUS, xSepRight + SIDE_PAD);
    const ownText = bandText.filter(t => {
      const cx = t.x + t.w / 2;
      return cx >= captureLeft && cx <= captureRight;
    });

    // ── compute content-extent of all own text ────────────────────────────────
    let contentX0 = item.x, contentX1 = item.x + item.w;
    let contentY0 = item.y, contentY1 = item.y + item.h;
    for (const t of ownText) {
      if (t.x             < contentX0) contentX0 = t.x;
      if (t.x + t.w       > contentX1) contentX1 = t.x + t.w;
      if (t.y             < contentY0) contentY0 = t.y;
      if (t.y + t.h       > contentY1) contentY1 = t.y + t.h;
    }

    // ── final crop rect ───────────────────────────────────────────────────────
    // X: clamp to separator midpoints (±3 pts tolerance) so neighbour profiles
    //    don't bleed in.  Y: use the full row band so vector drawings that
    //    extend above any dimension text are never clipped.
    const cropRect = [
      clamp(contentX0 - SIDE_PAD,   xSepLeft  - 3, pageW),
      clamp(bandY0,                  0,             pageH),
      clamp(contentX1 + SIDE_PAD,   0,             xSepRight + 3),
      clamp(bandY1,                  0,             pageH),
    ];

    // ── validate ──────────────────────────────────────────────────────────────
    const cropW = cropRect[2] - cropRect[0];
    const cropH = cropRect[3] - cropRect[1];
    if (cropW < 4 || cropH < 4) {
      console.warn(`  SKIP ${item.code}: degenerate ${cropW.toFixed(0)}×${cropH.toFixed(0)}`);
      skipped++;
      continue;
    }

    if (DRY_RUN) {
      console.log(`  [DRY] ${item.text}  [${cropRect.map(v => v.toFixed(1)).join(', ')}]  ${cropW.toFixed(0)}×${cropH.toFixed(0)} pts  ownText:${ownText.length}`);
      continue;
    }

    const outPath = join(OUTPUT_DIR, `${item.text}.png`);
    if (existsSync(outPath) && !FORCE) {
      console.log(`  SKIP ${item.text} (exists — use FORCE=1 to overwrite)`);
      skipped++;
      continue;
    }

    try {
      const png = renderCrop(dl, cropRect, SCALE);
      writeFileSync(outPath, png);
      const kb = (png.length / 1024).toFixed(0);
      const pw = Math.round(cropW * SCALE), ph = Math.round(cropH * SCALE);
      console.log(`  ✓ ${item.text}  ${Math.round(cropW)}×${Math.round(cropH)}pts → ${pw}×${ph}px  (${kb} KB)`);
      saved++;
    } catch (err) {
      console.error(`  ERROR ${item.text}:`, err.message);
      skipped++;
    }
  }
}

console.log(`\nDone.  Saved: ${saved}   Skipped/error: ${skipped}`);
console.log(`Output: ${OUTPUT_DIR}`);
