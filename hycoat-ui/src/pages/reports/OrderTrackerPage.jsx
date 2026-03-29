import { useState } from 'react';
import { Box, TextField, Grid } from '@mui/material';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ProgressBar from '@/components/reports/ProgressBar';
import StageChecklist from '@/components/reports/StageChecklist';
import ExportButton from '@/components/reports/ExportButton';
import { useOrderTracker, useExportReport } from '@/hooks/useReports';
import { formatDate } from '@/utils/formatters';

const columns = [
  { field: 'woNumber', headerName: 'WO Number' },
  { field: 'customerName', headerName: 'Customer' },
  { field: 'currentStage', headerName: 'Current Stage' },
  {
    field: 'completionPercent',
    headerName: 'Progress',
    renderCell: (row) => <ProgressBar value={row.completionPercent} />,
  },
  { field: 'daysInProcess', headerName: 'Days' },
  { field: 'woDate', headerName: 'WO Date', renderCell: (row) => formatDate(row.woDate) },
  {
    field: 'stages',
    headerName: 'Stages',
    sortable: false,
    renderCell: (row) => <StageChecklist row={row} />,
  },
];

export default function OrderTrackerPage() {
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [search, setSearch] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  const params = { page: page + 1, pageSize: rowsPerPage, search, dateFrom: dateFrom || undefined, dateTo: dateTo || undefined };
  const { data, isLoading } = useOrderTracker(params);
  const exportMutation = useExportReport();

  return (
    <Box>
      <PageHeader
        title="Order Tracker"
        action={
          <ExportButton
            onClick={() => exportMutation.mutate({ type: 'order-tracker', params: { search, dateFrom, dateTo } })}
            loading={exportMutation.isPending}
          />
        }
      />

      <Grid container spacing={2} sx={{ mb: 2 }}>
        <Grid size={{ xs: 12, md: 4 }}>
          <TextField fullWidth size="small" label="Search WO / Customer" value={search} onChange={(e) => { setSearch(e.target.value); setPage(0); }} />
        </Grid>
        <Grid size={{ xs: 6, md: 2 }}>
          <TextField fullWidth size="small" type="date" label="From" InputLabelProps={{ shrink: true }} value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} />
        </Grid>
        <Grid size={{ xs: 6, md: 2 }}>
          <TextField fullWidth size="small" type="date" label="To" InputLabelProps={{ shrink: true }} value={dateTo} onChange={(e) => setDateTo(e.target.value)} />
        </Grid>
      </Grid>

      <DataTable
        columns={columns}
        rows={data?.data?.items ?? []}
        loading={isLoading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={data?.data?.totalCount ?? 0}
        onPageChange={setPage}
        onRowsPerPageChange={(v) => { setRowsPerPage(v); setPage(0); }}
      />
    </Box>
  );
}
