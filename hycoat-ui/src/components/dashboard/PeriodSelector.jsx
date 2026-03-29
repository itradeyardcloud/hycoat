import { ToggleButtonGroup, ToggleButton, Box } from '@mui/material';

const PERIODS = [
  { value: 'today', label: 'Today' },
  { value: 'week', label: 'This Week' },
  { value: 'month', label: 'This Month' },
  { value: 'quarter', label: 'Quarter' },
  { value: 'year', label: 'Year' },
];

// eslint-disable-next-line react/prop-types
export default function PeriodSelector({ value, onChange }) {
  return (
    <Box>
      <ToggleButtonGroup
        value={value}
        exclusive
        onChange={(_, v) => v && onChange(v)}
        size="small"
      >
        {PERIODS.map((p) => (
          <ToggleButton key={p.value} value={p.value}>
            {p.label}
          </ToggleButton>
        ))}
      </ToggleButtonGroup>
    </Box>
  );
}
