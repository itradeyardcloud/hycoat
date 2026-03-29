import { useState } from 'react';
import { Grid, Box, CircularProgress, Alert } from '@mui/material';
import PageHeader from '@/components/common/PageHeader';
import KPICard from '@/components/dashboard/KPICard';
import DashboardChart from '@/components/dashboard/DashboardChart';
import PeriodSelector from '@/components/dashboard/PeriodSelector';
import YieldRoiCard from '@/components/dashboard/YieldRoiCard';
import { useAdminDashboard } from '@/hooks/useDashboard';
import { formatCurrency, formatNumber, formatSFT } from '@/utils/formatters';

export default function AdminDashboardPage() {
  const [period, setPeriod] = useState('month');
  const { data, isLoading, error } = useAdminDashboard({ period });
  const d = data?.data;

  if (isLoading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  if (error) return <Alert severity="error">Failed to load dashboard data.</Alert>;
  if (!d) return null;

  return (
    <Box>
      <PageHeader title="Admin Dashboard" action={<PeriodSelector value={period} onChange={setPeriod} />} />

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Active Work Orders" value={formatNumber(d.activeWorkOrders)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Pending Inquiries" value={formatNumber(d.pendingInquiries)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Monthly Revenue" value={formatCurrency(d.monthlyRevenue)} color="success.main" /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Monthly Production" value={formatSFT(d.monthlyProductionSFT)} /></Grid>
        <Grid size={{ xs: 12, md: 6 }}><YieldRoiCard targetPath="/reports/yield" /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Pending Dispatches" value={formatNumber(d.pendingDispatches)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Quality Pass Rate" value={`${d.qualityPassRate.toFixed(1)}%`} color={d.qualityPassRate >= 90 ? 'success.main' : 'warning.main'} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Low Stock Alerts" value={formatNumber(d.lowStockAlerts)} color={d.lowStockAlerts > 0 ? 'error.main' : 'success.main'} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Overdue Work Orders" value={formatNumber(d.overdueWorkOrders)} color={d.overdueWorkOrders > 0 ? 'error.main' : 'success.main'} /></Grid>
      </Grid>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart title="Revenue Trend" data={d.revenueTrend} type="line" />
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart title="Production Throughput (SFT)" data={d.productionThroughput} type="bar" />
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart title="Work Orders by Status" data={d.workOrdersByStatus} type="pie" dataKey="count" nameKey="status" />
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart title="Top Customers by Revenue" data={d.topCustomers?.map((c) => ({ label: c.customerName, value: c.revenue }))} type="bar" />
        </Grid>
      </Grid>
    </Box>
  );
}
