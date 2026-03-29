import { useState } from 'react';
import { Grid, Box, CircularProgress, Alert } from '@mui/material';
import PageHeader from '@/components/common/PageHeader';
import KPICard from '@/components/dashboard/KPICard';
import DashboardChart from '@/components/dashboard/DashboardChart';
import PeriodSelector from '@/components/dashboard/PeriodSelector';
import { useSalesDashboard } from '@/hooks/useDashboard';
import { formatNumber } from '@/utils/formatters';

export default function SalesDashboardPage() {
  const [period, setPeriod] = useState('month');
  const { data, isLoading, error } = useSalesDashboard({ period });
  const d = data?.data;

  if (isLoading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  if (error) return <Alert severity="error">Failed to load dashboard data.</Alert>;
  if (!d) return null;

  return (
    <Box>
      <PageHeader title="Sales Dashboard" action={<PeriodSelector value={period} onChange={setPeriod} />} />

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Open Inquiries" value={formatNumber(d.openInquiries)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Quotations Sent" value={formatNumber(d.quotationsSentThisMonth)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="PIs Awaiting Approval" value={formatNumber(d.pIsAwaitingApproval)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Active Work Orders" value={formatNumber(d.activeWorkOrders)} /></Grid>
        <Grid size={{ xs: 12, md: 3 }}><KPICard title="Quote → WO Rate" value={`${d.quotationToWOConversionRate.toFixed(1)}%`} color="info.main" /></Grid>
      </Grid>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart title="Inquiry Aging" data={d.inquiryAging?.map((a) => ({ label: a.bucket, value: a.count }))} type="bar" />
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart title="Monthly Quotations" data={d.monthlyQuotations} type="line" />
        </Grid>
      </Grid>
    </Box>
  );
}
