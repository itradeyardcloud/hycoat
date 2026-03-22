import dayjs from 'dayjs';

export const formatDate = (date) => date ? dayjs(date).format('DD/MM/YYYY') : '—';
export const formatDateTime = (date) => date ? dayjs(date).format('DD/MM/YYYY hh:mm A') : '—';
export const formatCurrency = (amount) => amount != null ? `₹${Number(amount).toLocaleString('en-IN', { minimumFractionDigits: 2 })}` : '—';
export const formatNumber = (num, decimals = 2) => num != null ? Number(num).toFixed(decimals) : '—';
export const formatSFT = (sft) => sft != null ? `${Number(sft).toFixed(2)} SFT` : '—';
