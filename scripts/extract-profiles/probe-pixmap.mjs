/**
 * Find Page.toPixmap signature in mupdf source
 */
import { readFileSync } from 'fs';

const src = readFileSync('./node_modules/mupdf/dist/mupdf.js', 'utf8');

// Find all toPixmap occurrences
let idx = 0;
const results = [];
while ((idx = src.indexOf('toPixmap', idx)) !== -1) {
  const snippet = src.slice(idx - 100, idx + 400);
  if (snippet.includes('class Page') || snippet.includes('Page.prototype') || 
      snippet.includes('_wasm_page') || snippet.includes('matrix') || snippet.includes('Matrix')) {
    results.push(src.slice(idx - 50, idx + 300));
  }
  idx += 8;
}

// Also find Page class definition
const pageClassIdx = src.indexOf('class Page ');
console.log('Page class location:', pageClassIdx);
if (pageClassIdx >= 0) {
  console.log('\nPage class:');
  console.log(src.slice(pageClassIdx, pageClassIdx + 2000));
}
