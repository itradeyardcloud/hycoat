import { Box, LinearProgress, Typography } from '@mui/material';

// eslint-disable-next-line react/prop-types
export default function ProgressBar({ value = 0 }) {
  const color = value >= 80 ? 'success' : value >= 50 ? 'warning' : 'error';

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, minWidth: 120 }}>
      <Box sx={{ flex: 1 }}>
        <LinearProgress variant="determinate" value={Math.min(value, 100)} color={color} sx={{ height: 8, borderRadius: 4 }} />
      </Box>
      <Typography variant="caption" fontWeight={600} sx={{ minWidth: 36 }}>
        {value.toFixed(0)}%
      </Typography>
    </Box>
  );
}
