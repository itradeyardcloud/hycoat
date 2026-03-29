import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Button,
  TextField,
  InputAdornment,
  IconButton,
  Box,
  Tooltip,
  Chip,
  Stack,
  MenuItem,
  Autocomplete,
} from '@mui/material';
import { Add, Search, Edit, Delete, PictureAsPdf } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { useQuotations, useDeleteQuotation } from '@/hooks/useQuotations';
import { useCustomerLookup } from '@/hooks/useCustomers';
import { quotationService } from '@/services/salesService';
import useDebounce from '@/hooks/useDebounce';

const STATUS_CONFIG = {
  Draft: { label: 'Draft', color: 'default' },
  Sent: { label: 'Sent', color: 'info' },
  Accepted: { label: 'Accepted', color: 'success' },
  Rejected: { label: 'Rejected', color: 'error' },
  Expired: { label: 'Expired', color: 'warning' },
};

function StatusChip({ status }) {
  const config = STATUS_CONFIG[status] || { label: status, color: 'default' };
  return <Chip label={config.label} color={config.color} size="small" />;
}

export default function QuotationsPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [statusFilter, setStatusFilter] = useState('');
  const [customerFilter, setCustomerFilter] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useQuotations({
    search: debouncedSearch || undefined,
    status: statusFilter || undefined,
    customerId: customerFilter?.id || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const { data: customers } = useCustomerLookup();
  const deleteMutation = useDeleteQuotation();

  const customerOptions = customers?.data ?? [];

  const handleDownloadPdf = async (e, row) => {
    e.stopPropagation();
    try {
      const response = await quotationService.getPdf(row.id);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const a = document.createElement('a');
      a.href = url;
      a.download = `${row.quotationNumber}.pdf`;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch {
      toast.error('Failed to download PDF');
    }
  };

  const columns = useMemo(
    () => [
      { field: 'quotationNumber', headerName: 'Quotation No' },
      {
        field: 'date',
        headerName: 'Date',
        renderCell: (row) => new Date(row.date).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' }),
      },
      { field: 'customerName', headerName: 'Customer' },
      { field: 'lineItemCount', headerName: 'Items' },
      {
        field: 'status',
        headerName: 'Status',
        renderCell: (row) => <StatusChip status={row.status} />,
      },
      {
        field: 'actions',
        headerName: '',
        sortable: false,
        renderCell: (row) => (
          <Box sx={{ display: 'flex', gap: 0.5 }}>
            <Tooltip title="Download PDF">
              <IconButton size="small" onClick={(e) => handleDownloadPdf(e, row)}>
                <PictureAsPdf fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Edit">
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/sales/quotations/${row.id}`);
                }}
              >
                <Edit fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Delete">
              <IconButton
                size="small"
                color="error"
                onClick={(e) => {
                  e.stopPropagation();
                  setDeleteTarget(row);
                }}
              >
                <Delete fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>
        ),
      },
    ],
    [navigate],
  );

  const handleDelete = () => {
    deleteMutation.mutate(deleteTarget.id, {
      onSuccess: () => {
        toast.success('Quotation deleted');
        setDeleteTarget(null);
      },
      onError: () => toast.error('Failed to delete quotation'),
    });
  };

  const rows = data?.data?.items ?? [];
  const totalCount = data?.data?.totalCount ?? 0;
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch && !statusFilter && !customerFilter;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Quotations" />
        <EmptyState
          title="No quotations yet"
          description="Create your first quotation to start sending proposals."
          action={
            <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/sales/quotations/new')}>
              New Quotation
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Quotations"
        action={
          <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/sales/quotations/new')}>
            New Quotation
          </Button>
        }
      />

      {/* Filters */}
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search quotations…"
          size="small"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(0);
          }}
          sx={{ minWidth: 240 }}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <Search />
                </InputAdornment>
              ),
            },
          }}
        />
        <TextField
          select
          size="small"
          label="Status"
          value={statusFilter}
          onChange={(e) => {
            setStatusFilter(e.target.value);
            setPage(0);
          }}
          sx={{ minWidth: 140 }}
        >
          <MenuItem value="">All</MenuItem>
          {Object.entries(STATUS_CONFIG).map(([key, config]) => (
            <MenuItem key={key} value={key}>
              {config.label}
            </MenuItem>
          ))}
        </TextField>
        <Autocomplete
          size="small"
          options={customerOptions}
          getOptionLabel={(o) => o.name}
          value={customerFilter}
          onChange={(_, val) => {
            setCustomerFilter(val);
            setPage(0);
          }}
          renderInput={(params) => <TextField {...params} label="Customer" />}
          sx={{ minWidth: 200 }}
          isOptionEqualToValue={(opt, val) => opt.id === val.id}
        />
      </Stack>

      <DataTable
        columns={columns}
        rows={rows}
        loading={isLoading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={totalCount}
        onPageChange={setPage}
        onRowsPerPageChange={(size) => {
          setRowsPerPage(size);
          setPage(0);
        }}
        onRowClick={(row) => navigate(`/sales/quotations/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Quotation"
        message={`Are you sure you want to delete "${deleteTarget?.quotationNumber}"? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
