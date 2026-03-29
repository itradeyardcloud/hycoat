import { Box, Chip, Stack } from '@mui/material';

const STAGES = [
  { key: 'inquiryCreated', label: 'Inquiry' },
  { key: 'quotationSent', label: 'Quotation' },
  { key: 'piApproved', label: 'PI' },
  { key: 'workOrderCreated', label: 'WO' },
  { key: 'materialReceived', label: 'Material' },
  { key: 'inspectionDone', label: 'Inspection' },
  { key: 'pwoCreated', label: 'PWO' },
  { key: 'productionDone', label: 'Production' },
  { key: 'qualityApproved', label: 'QA' },
  { key: 'dispatched', label: 'Dispatch' },
  { key: 'invoiced', label: 'Invoice' },
];

// eslint-disable-next-line react/prop-types
export default function StageChecklist({ row }) {
  return (
    <Box sx={{ py: 1 }}>
      <Stack direction="row" spacing={0.5} flexWrap="wrap" useFlexGap>
        {STAGES.map((s) => (
          <Chip
            key={s.key}
            label={s.label}
            size="small"
            color={row[s.key] ? 'success' : 'default'}
            variant={row[s.key] ? 'filled' : 'outlined'}
          />
        ))}
      </Stack>
    </Box>
  );
}
