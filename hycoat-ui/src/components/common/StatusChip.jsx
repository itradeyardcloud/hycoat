import { Chip } from '@mui/material';

const statusColorMap = {
  // Default (gray)
  New: 'default',
  Created: 'default',
  Draft: 'default',
  // Info (blue)
  InProgress: 'info',
  InProduction: 'info',
  Scheduled: 'info',
  MaterialAwaited: 'info',
  MaterialReceived: 'info',
  QuotationSent: 'info',
  BOMReceived: 'info',
  PISent: 'info',
  // Success (green)
  Complete: 'success',
  Completed: 'success',
  Approved: 'success',
  Pass: 'success',
  Confirmed: 'success',
  QAComplete: 'success',
  Dispatched: 'success',
  Invoiced: 'success',
  Closed: 'success',
  // Error (red)
  Rejected: 'error',
  Fail: 'error',
  Failed: 'error',
  Cancelled: 'error',
  Lost: 'error',
  // Warning (orange)
  Pending: 'warning',
  Awaited: 'warning',
};

function getChipColor(status) {
  return statusColorMap[status] || 'default';
}

export default function StatusChip({ status, size = 'small' }) {
  return (
    <Chip
      label={status}
      color={getChipColor(status)}
      size={size}
      variant="outlined"
    />
  );
}
