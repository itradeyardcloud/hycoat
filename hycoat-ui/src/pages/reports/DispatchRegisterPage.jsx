import { useState } from 'react';
import { Box, Grid, TextField } from '@mui/material';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ExportButton from '@/components/reports/ExportButton';
import { useDispatchRegister, useExportReport } from '@/hooks/useReports';
import { formatDate } from '@/utils/formatters';

const columns = [
  { field: 'dcNumber', headerName: 'DC Number' },
  { field: 'dcDate', headerName: 'DC Date', renderCell: (row) => formatDate(row.dcDate) },
  { field: 'customerName', headerName: 'Customer' },
  { field: 'woNumber', headerName: 'WO #' },
  { field: 'vehicleNumber', headerName: 'Vehicle' },
  { field: 'status', headerName: 'Status' },
  { field: 'totalPackages', headerName: 'Packages' },
];

export default function DispatchRegisterPage() {
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  const params = { page: page + 1, pageSize: rowsPerPage, dateFrom: dateFrom || undefined, dateTo: dateTo || undefined };
  const { data, isLoading } = useDispatchRegister(params);
  const exportMutation = useExportReport();

  return (
    <Box>
      <PageHeader
        title="Dispatch Register"
        action={
          <ExportButton
            onClick={() => exportMutation.mutate({ type: 'dispatch-register', params: { dateFrom, dateTo } })}
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
