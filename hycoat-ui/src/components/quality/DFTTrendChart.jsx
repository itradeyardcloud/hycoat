import { useState } from 'react';
import { Box, Autocomplete, TextField, Typography, CircularProgress } from '@mui/material';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ReferenceLine,
  ResponsiveContainer,
  Dot,
} from 'recharts';
import { useDFTTrend } from '@/hooks/useInProcessInspections';
import { useProductionWorkOrderLookup } from '@/hooks/useProductionWorkOrders';

const DFT_MIN = 60;
const DFT_MAX = 80;

// eslint-disable-next-line react/prop-types
function OutOfSpecDot({ cx, cy, payload, ...rest }) {
  // eslint-disable-next-line react/prop-types
  if (!payload || payload.isWithinSpec) return <Dot cx={cx} cy={cy} {...rest} />;
  return <Dot cx={cx} cy={cy} {...rest} r={5} fill="#d32f2f" stroke="#d32f2f" />;
}

export default function DFTTrendChart() {
  const [selectedPWO, setSelectedPWO] = useState(null);
  const { data: pwos } = useProductionWorkOrderLookup();
  const { data: trendData, isLoading } = useDFTTrend(selectedPWO?.id);

  const pwoOptions = pwos?.data ?? [];
  const points = (trendData?.data ?? []).map((p) => ({
    ...p,
    label: new Date(p.date).toLocaleDateString('en-IN', { day: '2-digit', month: 'short' }) +
      (p.time ? ` ${p.time.split(':').slice(0, 2).join(':')}` : ''),
  }));

  return (
    <Box>
      <Autocomplete
        size="small"
        options={pwoOptions}
        getOptionLabel={(o) => o.name}
        value={selectedPWO}
        onChange={(_, val) => setSelectedPWO(val)}
        renderInput={(params) => <TextField {...params} label="Select PWO for DFT Trend" />}
        sx={{ maxWidth: 320, mb: 2 }}
        isOptionEqualToValue={(opt, val) => opt.id === val.id}
      />

      {!selectedPWO && (
        <Typography color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
          Select a Production Work Order to view DFT trend
        </Typography>
      )}

      {selectedPWO && isLoading && (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      )}

      {selectedPWO && !isLoading && points.length === 0 && (
        <Typography color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
          No DFT readings found for this PWO
        </Typography>
      )}

      {selectedPWO && !isLoading && points.length > 0 && (
        <ResponsiveContainer width="100%" height={350}>
          <LineChart data={points} margin={{ top: 10, right: 20, left: 10, bottom: 10 }}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="label" fontSize={11} />
            <YAxis domain={[0, 'auto']} label={{ value: 'µm', angle: -90, position: 'insideLeft' }} />
            <Tooltip />
            <ReferenceLine y={DFT_MIN} stroke="#ff9800" strokeDasharray="5 5" label={{ value: `Min ${DFT_MIN}`, position: 'left', fontSize: 11 }} />
            <ReferenceLine y={DFT_MAX} stroke="#ff9800" strokeDasharray="5 5" label={{ value: `Max ${DFT_MAX}`, position: 'left', fontSize: 11 }} />
            <Line type="monotone" dataKey="avgReading" name="DFT Avg" stroke="#1976d2" strokeWidth={2} dot={<OutOfSpecDot />} />
            <Line type="monotone" dataKey="minReading" name="DFT Min" stroke="#9e9e9e" strokeDasharray="3 3" dot={false} />
            <Line type="monotone" dataKey="maxReading" name="DFT Max" stroke="#9e9e9e" strokeDasharray="3 3" dot={false} />
          </LineChart>
        </ResponsiveContainer>
      )}
    </Box>
  );
}
