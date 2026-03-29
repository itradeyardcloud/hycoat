import { useState } from 'react';
import { Box, Grid, TextField } from '@mui/material';
import PageHeader from '@/components/common/PageHeader';
import KPICard from '@/components/dashboard/KPICard';
import DashboardChart from '@/components/dashboard/DashboardChart';
import DataTable from '@/components/common/DataTable';
import ExportButton from '@/components/reports/ExportButton';
import { useProductionThroughput, useExportReport } from '@/hooks/useReports';
import { formatNumber, formatSFT, formatDate } from '@/utils/formatters';

const columns = [
  { field: 'date', headerName: 'Date', renderCell: (row) => formatDate(row.date) },
  { field: 'pwoNumber', headerName: 'PWO #' },
  { field: 'customerName', headerName: 'Customer' },
  { field: 'totalSFT', headerName: 'SFT', renderCell: (row) => formatSFT(row.totalSFT) },
  { field: 'basketCount', headerName: 'Baskets' },
  { field: 'shift', headerName: 'Shift' },
];

export default function ProductionReportPage() {
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  const params = { dateFrom: dateFrom || undefined, dateTo: dateTo || undefined };
  const { data, isLoading } = useProductionThroughput(params);
  const exportMutation = useExportReport();
  const d = data?.data;

  return (
    <Box>
      <PageHeader
        title="Production Throughput Report"
        action={
          <ExportButton
            onClick={() => exportMutation.mutate({ type: 'production-throughput', params })}
            loading={exportMutation.isPending}
          />
        }
      />

      <Grid container spacing={2} sx={{ mb: 2 }}>
        <Grid size={{ xs: 6, md: 2 }}>
          <TextField fullWidth size="small" type="date" label="From" InputLabelProps={{ shrink: true }} value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} />
        </Grid>
        <Grid size={{ xs: 6, md: 2 }}>
          <TextField fullWidth size="small" type="date" label="To" InputLabelProps={{ shrink: true }} value={dateTo} onChange={(e) => setDateTo(e.target.value)} />
        </Grid>
      </Grid>

      {d && (
        <>
          <Grid container spacing={2} sx={{ mb: 3 }}>
            <Grid size={{ xs: 6, md: 3 }}><KPICard title="Total SFT" value={formatSFT(d.totalSFT)} color="success.main" /></Grid>
            <Grid size={{ xs: 6, md: 3 }}><KPICard title="Total Baskets" value={formatNumber(d.totalBaskets)} /></Grid>
            <Grid size={{ xs: 6, md: 3 }}><KPICard title="Working Days" value={formatNumber(d.workingDays)} /></Grid>
            <Grid size={{ xs: 6, md: 3 }}><KPICard title="Avg SFT / Day" value={formatSFT(d.avgSFTPerDay)} /></Grid>
          </Grid>

          <Grid container spacing={2} sx={{ mb: 3 }}>
            <Grid size={{ xs: 12 }}>
              <DashboardChart title="Daily Throughput (SFT)" data={d.dailyTrend} type="line" height={300} />
            </Grid>
          </Grid>

          <DataTable columns={columns} rows={d.details ?? []} loading={isLoading} />
        </>
      )}
    </Box>
  );
}
