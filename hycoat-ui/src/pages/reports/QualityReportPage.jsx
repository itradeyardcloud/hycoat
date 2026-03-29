import { useState } from 'react';
import { Box, Grid, TextField } from '@mui/material';
import PageHeader from '@/components/common/PageHeader';
import KPICard from '@/components/dashboard/KPICard';
import DashboardChart from '@/components/dashboard/DashboardChart';
import { useQualitySummary } from '@/hooks/useReports';
import { formatNumber } from '@/utils/formatters';

export default function QualityReportPage() {
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  const params = { dateFrom: dateFrom || undefined, dateTo: dateTo || undefined };
  const { data, isLoading } = useQualitySummary(params);
  const d = data?.data;

  return (
    <Box>
      <PageHeader title="Quality Summary Report" />

      <Grid container spacing={2} sx={{ mb: 2 }}>
        <Grid size={{ xs: 6, md: 2 }}>
          <TextField fullWidth size="small" type="date" label="From" InputLabelProps={{ shrink: true }} value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} />
        </Grid>
        <Grid size={{ xs: 6, md: 2 }}>
          <TextField fullWidth size="small" type="date" label="To" InputLabelProps={{ shrink: true }} value={dateTo} onChange={(e) => setDateTo(e.target.value)} />
        </Grid>
      </Grid>

      {!isLoading && d && (
        <>
          <Grid container spacing={2} sx={{ mb: 3 }}>
            <Grid size={{ xs: 6, md: 3 }}><KPICard title="Total Inspections" value={formatNumber(d.totalInspections)} /></Grid>
            <Grid size={{ xs: 6, md: 3 }}><KPICard title="Pass Rate" value={`${d.passRate.toFixed(1)}%`} color={d.passRate >= 90 ? 'success.main' : 'error.main'} /></Grid>
            <Grid size={{ xs: 6, md: 3 }}><KPICard title="DFT Pass Rate" value={`${d.dftPassRate.toFixed(1)}%`} /></Grid>
            <Grid size={{ xs: 6, md: 3 }}><KPICard title="Reworks" value={formatNumber(d.reworkCount)} color={d.reworkCount > 0 ? 'warning.main' : 'success.main'} /></Grid>
          </Grid>

          <Grid container spacing={2}>
            <Grid size={{ xs: 12, md: 6 }}>
              <DashboardChart title="DFT Trend" data={d.dftTrend} type="line" />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <DashboardChart title="Inspection Results" data={d.resultBreakdown} type="pie" dataKey="count" nameKey="status" />
            </Grid>
          </Grid>
        </>
      )}
    </Box>
  );
}
