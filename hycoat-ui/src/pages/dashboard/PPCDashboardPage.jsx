import { useState } from 'react';
import { Grid, Box, CircularProgress, Alert } from '@mui/material';
import PageHeader from '@/components/common/PageHeader';
import KPICard from '@/components/dashboard/KPICard';
import DashboardChart from '@/components/dashboard/DashboardChart';
import PeriodSelector from '@/components/dashboard/PeriodSelector';
import { usePPCDashboard } from '@/hooks/useDashboard';
import { formatNumber, formatSFT } from '@/utils/formatters';

export default function PPCDashboardPage() {
  const [period, setPeriod] = useState('month');
  const { data, isLoading, error } = usePPCDashboard({ period });
  const d = data?.data;

  if (isLoading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  if (error) return <Alert severity="error">Failed to load dashboard data.</Alert>;
  if (!d) return null;

  return (
    <Box>
      <PageHeader title="PPC Dashboard" action={<PeriodSelector value={period} onChange={setPeriod} />} />

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Active PWOs" value={formatNumber(d.activePWOs)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Unscheduled PWOs" value={formatNumber(d.unscheduledPWOs)} color={d.unscheduledPWOs > 0 ? 'warning.main' : 'success.main'} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Week Utilization" value={`${d.weekUtilizationPercent.toFixed(1)}%`} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Scheduled SFT" value={formatSFT(d.totalScheduledSFT)} /></Grid>
      </Grid>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart title="Weekly Schedule Load" data={d.weeklyScheduleLoad} type="bar" />
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart title="PWOs by Status" data={d.pwOsByStatus} type="pie" dataKey="count" nameKey="status" />
        </Grid>
      </Grid>
    </Box>
  );
}
