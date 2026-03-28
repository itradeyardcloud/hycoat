const ones = ['', 'One', 'Two', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine',
  'Ten', 'Eleven', 'Twelve', 'Thirteen', 'Fourteen', 'Fifteen', 'Sixteen', 'Seventeen', 'Eighteen', 'Nineteen'];
const tens = ['', '', 'Twenty', 'Thirty', 'Forty', 'Fifty', 'Sixty', 'Seventy', 'Eighty', 'Ninety'];

function twoDigits(n) {
  if (n < 20) return ones[n];
  return tens[Math.floor(n / 10)] + (n % 10 ? ' ' + ones[n % 10] : '');
}

function threeDigits(n) {
  if (n === 0) return '';
  if (n < 100) return twoDigits(n);
  return ones[Math.floor(n / 100)] + ' Hundred' + (n % 100 ? ' and ' + twoDigits(n % 100) : '');
}

/**
 * Convert a number to Indian currency words (Rupees and Paise).
 */
export function numberToWords(amount) {
  if (amount == null || isNaN(amount)) return '';
  if (amount === 0) return 'Zero Rupees Only';

  const abs = Math.abs(amount);
  const rupees = Math.floor(abs);
  const paise = Math.round((abs - rupees) * 100);

  let result = '';

  if (rupees > 0) {
    const crore = Math.floor(rupees / 10000000);
    const lakh = Math.floor((rupees % 10000000) / 100000);
    const thousand = Math.floor((rupees % 100000) / 1000);
    const hundred = rupees % 1000;

    const parts = [];
    if (crore) parts.push(threeDigits(crore) + ' Crore');
    if (lakh) parts.push(twoDigits(lakh) + ' Lakh');
    if (thousand) parts.push(twoDigits(thousand) + ' Thousand');
    if (hundred) parts.push(threeDigits(hundred));

    result = parts.join(' ') + ' Rupees';
  }

  if (paise > 0) {
    result += (result ? ' and ' : '') + twoDigits(paise) + ' Paise';
  }

  return result + ' Only';
}
