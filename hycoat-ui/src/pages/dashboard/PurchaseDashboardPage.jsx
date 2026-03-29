import { useState } from 'react';
import { Grid, Box, CircularProgress, Alert } from '@mui/material';
import PageHeader from '@/components/common/PageHeader';
import KPICard from '@/components/dashboard/KPICard';
import DashboardChart from '@/components/dashboard/DashboardChart';
import PeriodSelector from '@/components/dashboard/PeriodSelector';
import { usePurchaseDashboard } from '@/hooks/useDashboard';
import { formatNumber } from '@/utils/formatters';

export default function PurchaseDashboardPage() {
  const [period, setPeriod] = useState('month');
  const { data, isLoading, error } = usePurchaseDashboard({ period });
  const d = data?.data;

  if (isLoading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  if (error) return <Alert severity="error">Failed to load dashboard data.</Alert>;
  if (!d) return null;

  return (
    <Box>
      <PageHeader title="Purchase Dashboard" action={<PeriodSelector value={period} onChange={setPeriod} />} />

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Open Indents" value={formatNumber(d.openIndents)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Pending POs" value={formatNumber(d.pendingPOs)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Low Stock Items" value={formatNumber(d.lowStockItems)} color={d.lowStockItems > 0 ? 'error.main' : 'success.main'} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Total Powder Stock" value={`${formatNumber(d.totalPowderStockKg)} kg`} /></Grid>
      </Grid>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart
            title="Low Stock Alerts"
            data={d.lowStockAlerts?.map((s) => ({ label: `${s.powderCode} - ${s.colorName}`, value: s.currentStockKg }))}
            type="bar"
          />
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart title="Monthly Purchase Spend" data={d.monthlyPurchaseSpend} type="line" />
        </Grid>
      </Grid>
    </Box>
  );
}
