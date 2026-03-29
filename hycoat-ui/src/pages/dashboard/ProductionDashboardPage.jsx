import { useState } from 'react';
import { Grid, Box, CircularProgress, Alert } from '@mui/material';
import PageHeader from '@/components/common/PageHeader';
import KPICard from '@/components/dashboard/KPICard';
import DashboardChart from '@/components/dashboard/DashboardChart';
import PeriodSelector from '@/components/dashboard/PeriodSelector';
import { useProductionDashboard } from '@/hooks/useDashboard';
import { formatNumber, formatSFT } from '@/utils/formatters';

export default function ProductionDashboardPage() {
  const [period, setPeriod] = useState('today');
  const { data, isLoading, error } = useProductionDashboard({ period });
  const d = data?.data;

  if (isLoading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  if (error) return <Alert severity="error">Failed to load dashboard data.</Alert>;
  if (!d) return null;

  return (
    <Box>
      <PageHeader title="Production Dashboard" action={<PeriodSelector value={period} onChange={setPeriod} />} />

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Baskets Processed" value={formatNumber(d.basketsProcessedToday)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="SFT Coated" value={formatSFT(d.sftCoatedToday)} color="success.main" /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Active Logs" value={formatNumber(d.activeProductionLogs)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Avg Conveyor Speed" value={`${d.avgConveyorSpeed.toFixed(1)}`} /></Grid>
      </Grid>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12 }}>
          <DashboardChart title="Daily Output (SFT)" data={d.dailyOutput} type="line" height={350} />
        </Grid>
      </Grid>
    </Box>
  );
}
