import { useState } from 'react';
import { Box, Grid, Autocomplete, TextField, Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import PageHeader from '@/components/common/PageHeader';
import KPICard from '@/components/dashboard/KPICard';
import DataTable from '@/components/common/DataTable';
import { useCustomerHistory } from '@/hooks/useReports';
import { formatDate, formatCurrency, formatSFT } from '@/utils/formatters';
import api from '@/services/api';

const columns = [
  { field: 'woNumber', headerName: 'WO #' },
  { field: 'woDate', headerName: 'WO Date', renderCell: (row) => formatDate(row.woDate) },
  { field: 'status', headerName: 'Status' },
  { field: 'totalSFT', headerName: 'Area (SFT)', renderCell: (row) => formatSFT(row.totalSFT) },
  { field: 'invoiceAmount', headerName: 'Invoice Amt', renderCell: (row) => formatCurrency(row.invoiceAmount) },
  { field: 'dispatchDate', headerName: 'Dispatch Date', renderCell: (row) => row.dispatchDate ? formatDate(row.dispatchDate) : '-' },
];

export default function CustomerHistoryPage() {
  const [customerId, setCustomerId] = useState(null);
  const [search, setSearch] = useState('');

  const { data: customerOptions } = useQuery({
    queryKey: ['customers-dropdown', search],
    queryFn: () => api.get('/customers', { params: { search, pageSize: 20 } }).then((r) => r.data?.data?.items ?? []),
    enabled: search.length >= 1,
  });

  const { data, isLoading } = useCustomerHistory(customerId);
  const d = data?.data;

  return (
    <Box>
      <PageHeader title="Customer History" />

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, md: 4 }}>
          <Autocomplete
            options={customerOptions ?? []}
            getOptionLabel={(o) => o.name ?? ''}
            isOptionEqualToValue={(o, v) => o.id === v.id}
            onInputChange={(_, v) => setSearch(v)}
            onChange={(_, v) => setCustomerId(v?.id ?? null)}
            renderInput={(params) => <TextField {...params} label="Select Customer" size="small" />}
          />
        </Grid>
      </Grid>

      {d && (
        <>
          <Grid container spacing={2} sx={{ mb: 3 }}>
            <Grid size={{ xs: 6, md: 3 }}><KPICard title="Customer" value={d.customerName} /></Grid>
            <Grid size={{ xs: 6, md: 3 }}><KPICard title="Total Orders" value={d.totalOrders} /></Grid>
            <Grid size={{ xs: 6, md: 3 }}><KPICard title="Total Revenue" value={formatCurrency(d.totalRevenue)} color="success.main" /></Grid>
            <Grid size={{ xs: 6, md: 3 }}><KPICard title="Total SFT" value={formatSFT(d.totalSFT)} /></Grid>
          </Grid>

          <DataTable columns={columns} rows={d.orders ?? []} loading={isLoading} />
        </>
      )}

      {!customerId && (
        <Typography color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
          Select a customer to view their order history.
        </Typography>
      )}
    </Box>
  );
}
