/**
 * Probe the mupdf page API and find all profile codes with their positions.
 */
import mupdf from 'mupdf';
import { readFileSync } from 'fs';

const PDF = 'D:/IProject/ToBeDone/549-R1_Master Profile Chart of All Aluminium System-R1-(20.01.26) (1).pdf';

const buf = readFileSync(PDF);
const doc = mupdf.Document.openDocument(buf, 'application/pdf');
console.log('Pages:', doc.countPages());

const page = doc.loadPage(0);
const bounds = page.getBounds();
console.log('Page bounds (pts):', bounds);
console.log('Page size mm:', (bounds[2] / 2.835).toFixed(1), 'x', (bounds[3] / 2.835).toFixed(1));

const dl = page.toDisplayList();
const stext = dl.toStructuredText('preserve-whitespace,preserve-spans');
const json = JSON.parse(stext.asJSON());
console.log('Total text blocks:', json.blocks.length);

// Find profile codes: pattern like AS35PE03, AL56PF01, AA00PE04, AC39PB01, EX-1251148800
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

console.log('Profile codes found:', codes.length);
console.log('Sample (first 10):');
codes.slice(0, 10).forEach(c => {
  const b = c.bbox;
  console.log(`  ${c.code}  x=${b.x.toFixed(1)} y=${b.y.toFixed(1)} w=${b.w.toFixed(1)} h=${b.h.toFixed(1)}`);
});

// Show page methods
const pageProto = Object.getOwnPropertyNames(Object.getPrototypeOf(page));
console.log('\nPage methods:', pageProto.join(', '));

// Check toPixmap signature by looking at source snippet
const fs2 = await import('fs');
const mupdfSrc = fs2.readFileSync('./node_modules/mupdf/dist/mupdf.js', 'utf8');
const toPixmapIdx = mupdfSrc.indexOf('toPixmap');
console.log('\ntoPixmap in mupdf.js context:');
console.log(mupdfSrc.slice(toPixmapIdx - 20, toPixmapIdx + 300));
