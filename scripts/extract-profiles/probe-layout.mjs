/**
 * Probe profile code layout: analyze X/Y distribution to determine correct crop padding
 */
import mupdf from 'mupdf';
import { readFileSync } from 'fs';

const PDF = 'D:/IProject/ToBeDone/549-R1_Master Profile Chart of All Aluminium System-R1-(20.01.26) (1).pdf';
const buf = readFileSync(PDF);
const doc = mupdf.Document.openDocument(buf, 'application/pdf');
const page = doc.loadPage(0);
const dl = page.toDisplayList();
const stext = dl.toStructuredText('preserve-whitespace,preserve-spans');
const json = JSON.parse(stext.asJSON());

const codePattern = /^[A-Z]{2}\d{2,3}[A-Z]{2}\d{2,3}$/;
const codes = [];
for (const block of json.blocks) {
  if (!block.lines) continue;
  for (const line of block.lines) {
    const text = (line.text || '').trim();
    if (codePattern.test(text)) {
      codes.push({ code: text, bbox: line.bbox });
    }
  }
}
console.log('Total codes:', codes.length);

// Sort by Y then X
codes.sort((a, b) => a.bbox.y - b.bbox.y || a.bbox.x - b.bbox.x);

// Analyze Y positions
const ys = codes.map(c => c.bbox.y);
console.log('\nY range:', Math.min(...ys).toFixed(0), 'to', Math.max(...ys).toFixed(0));

// Group by Y proximity (within 5pts = same row)
const rows = [];
let currentRow = [codes[0]];
for (let i = 1; i < codes.length; i++) {
  if (Math.abs(codes[i].bbox.y - codes[i-1].bbox.y) < 8) {
    currentRow.push(codes[i]);
  } else {
    rows.push(currentRow);
    currentRow = [codes[i]];
  }
}
rows.push(currentRow);
console.log('\nRows found:', rows.length);

// For each row, show Y position and code count
for (const row of rows) {
  const ys2 = row.map(r => r.bbox.y);
  const xs = row.map(r => r.bbox.x);
  const minY = Math.min(...ys2);
  const maxY = Math.max(...ys2);
  console.log(`  Row y≈${minY.toFixed(0)}-${maxY.toFixed(0)}: ${row.length} codes, X range ${Math.min(...xs).toFixed(0)}-${Math.max(...xs).toFixed(0)}`);
  
  // Analyze X spacing within row
  const sortedByX = [...row].sort((a, b) => a.bbox.x - b.bbox.x);
  const xGaps = [];
  for (let i = 1; i < sortedByX.length; i++) {
    xGaps.push(sortedByX[i].bbox.x - (sortedByX[i-1].bbox.x + sortedByX[i-1].bbox.w));
  }
  if (xGaps.length > 0) {
    const avgGap = xGaps.reduce((a, b) => a + b, 0) / xGaps.length;
    const minGap = Math.min(...xGaps);
    const maxGap = Math.max(...xGaps);
    console.log(`    X gaps: avg=${avgGap.toFixed(1)}, min=${minGap.toFixed(1)}, max=${maxGap.toFixed(1)}`);
  }
}

// Analyze Y gaps between rows
const rowYs = rows.map(r => r[0].bbox.y);
const yGaps = [];
for (let i = 1; i < rowYs.length; i++) {
  yGaps.push(rowYs[i] - rowYs[i-1]);
}
console.log('\nY gaps between rows:', yGaps.map(g => g.toFixed(1)).join(', '));
console.log('Page bounds:', page.getBounds());
