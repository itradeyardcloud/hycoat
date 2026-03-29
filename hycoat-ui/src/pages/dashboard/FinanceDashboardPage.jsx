import { useState } from 'react';
import { Grid, Box, CircularProgress, Alert } from '@mui/material';
import PageHeader from '@/components/common/PageHeader';
import KPICard from '@/components/dashboard/KPICard';
import DashboardChart from '@/components/dashboard/DashboardChart';
import PeriodSelector from '@/components/dashboard/PeriodSelector';
import { useFinanceDashboard } from '@/hooks/useDashboard';
import { formatCurrency, formatNumber } from '@/utils/formatters';

export default function FinanceDashboardPage() {
  const [period, setPeriod] = useState('month');
  const { data, isLoading, error } = useFinanceDashboard({ period });
  const d = data?.data;

  if (isLoading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  if (error) return <Alert severity="error">Failed to load dashboard data.</Alert>;
  if (!d) return null;

  return (
    <Box>
      <PageHeader title="Finance Dashboard" action={<PeriodSelector value={period} onChange={setPeriod} />} />

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Monthly Invoiced" value={formatCurrency(d.monthlyInvoicedAmount)} color="success.main" /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Invoices Sent" value={formatNumber(d.invoicesSentThisMonth)} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Unpaid Invoices" value={formatNumber(d.unpaidInvoices)} color={d.unpaidInvoices > 0 ? 'warning.main' : 'success.main'} /></Grid>
        <Grid size={{ xs: 6, md: 3 }}><KPICard title="Outstanding" value={formatCurrency(d.outstandingAmount)} color="error.main" /></Grid>
      </Grid>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart title="Monthly Revenue" data={d.monthlyRevenue} type="line" />
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <DashboardChart title="Invoices by Status" data={d.invoicesByStatus} type="pie" dataKey="count" nameKey="status" />
        </Grid>
      </Grid>
    </Box>
  );
}
