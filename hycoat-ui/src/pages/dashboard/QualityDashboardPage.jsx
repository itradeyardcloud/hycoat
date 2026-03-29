import { useState } from 'react';
import { Grid, Box, CircularProgress, Alert } from '@mui/material';
import PageHeader from '@/components/common/PageHeader';
import KPICard from '@/components/dashboard/KPICard';
import DashboardChart from '@/components/dashboard/DashboardChart';
import PeriodSelector from '@/components/dashboard/PeriodSelector';
import { useQualityDashboard } from '@/hooks/useDashboard';
import { formatNumber } from '@/utils/formatters';

export default function QualityDashboardPage() {
  const [period, setPeriod] = useState('month');
  const { data, isLoading, error } = useQualityDashboard({ period });
  const d = data?.data;

  if (isLoading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  if (error) return <Alert severity="error">Failed to load dashboard data.</Alert>;
  if (!d) return null;

  return (
    <Box>
      <PageHeader title="Quality Dashboard" action={<PeriodSelector value={period} onChange={setPeriod} />} />

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Inspections Today" value={formatNumber(d.inspectionsToday)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="DFT Pass Rate" value={`${d.dftPassRate.toFixed(1)}%`} color={d.dftPassRate >= 90 ? 'success.main' : 'warning.main'} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Pending Final" value={formatNumber(d.pendingFinalInspections)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="TCs Issued" value={formatNumber(d.testCertificatesIssued)} /></Grid>
        <Grid size={{ xs: 12, md: 3 }}><KPICard title="Overall Pass Rate" value={`${d.overallPassRate.toFixed(1)}%`} color={d.overallPassRate >= 90 ? 'success.main' : 'error.main'} /></Grid>
      </Grid>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart title="DFT Trend" data={d.dftTrend} type="line" />
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart title="Inspection Results" data={d.inspectionResults} type="pie" dataKey="count" nameKey="status" />
        </Grid>
      </Grid>
    </Box>
  );
}
