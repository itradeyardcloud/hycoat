import { useState } from 'react';
import { Grid, Box, CircularProgress, Alert } from '@mui/material';
import PageHeader from '@/components/common/PageHeader';
import KPICard from '@/components/dashboard/KPICard';
import DashboardChart from '@/components/dashboard/DashboardChart';
import PeriodSelector from '@/components/dashboard/PeriodSelector';
import { useSCMDashboard } from '@/hooks/useDashboard';
import { formatNumber } from '@/utils/formatters';

export default function SCMDashboardPage() {
  const [period, setPeriod] = useState('week');
  const { data, isLoading, error } = useSCMDashboard({ period });
  const d = data?.data;

  if (isLoading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  if (error) return <Alert severity="error">Failed to load dashboard data.</Alert>;
  if (!d) return null;

  return (
    <Box>
      <PageHeader title="SCM Dashboard" action={<PeriodSelector value={period} onChange={setPeriod} />} />

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Materials Received Today" value={formatNumber(d.materialsReceivedToday)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Pending Dispatches" value={formatNumber(d.pendingDispatches)} color={d.pendingDispatches > 0 ? 'warning.main' : 'success.main'} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Challans Drafted Today" value={formatNumber(d.challansDraftedToday)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Dispatched This Week" value={formatNumber(d.dispatchedThisWeek)} color="success.main" /></Grid>
      </Grid>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12 }}>
          <DashboardChart title="Delivery Challans by Status" data={d.dcsByStatus} type="pie" dataKey="count" nameKey="status" height={350} />
        </Grid>
      </Grid>
    </Box>
  );
}
