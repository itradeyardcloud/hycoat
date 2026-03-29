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
import { useProformaInvoices, useDeletePI } from '@/hooks/useProformaInvoices';
import { useCustomerLookup } from '@/hooks/useCustomers';
import { piService } from '@/services/salesService';
import { formatCurrency } from '@/utils/formatters';
import useDebounce from '@/hooks/useDebounce';

const STATUS_CONFIG = {
  Draft: { label: 'Draft', color: 'default' },
  Sent: { label: 'Sent', color: 'info' },
  Accepted: { label: 'Accepted', color: 'success' },
  Rejected: { label: 'Rejected', color: 'error' },
};

function StatusChip({ status }) {
  const config = STATUS_CONFIG[status] || { label: status, color: 'default' };
  return <Chip label={config.label} color={config.color} size="small" />;
}

export default function PIListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [statusFilter, setStatusFilter] = useState('');
  const [customerFilter, setCustomerFilter] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useProformaInvoices({
    search: debouncedSearch || undefined,
    status: statusFilter || undefined,
    customerId: customerFilter?.id || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const { data: customers } = useCustomerLookup();
  const deleteMutation = useDeletePI();

  const customerOptions = customers?.data ?? [];

  const handleDownloadPdf = async (e, row) => {
    e.stopPropagation();
    try {
      const response = await piService.getPdf(row.id);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const a = document.createElement('a');
      a.href = url;
      a.download = `${row.piNumber || row.pINumber || `PI-${row.id}`}.pdf`;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch {
      toast.error('Failed to download PDF');
    }
  };

  const columns = useMemo(
    () => [
      { field: 'piNumber', headerName: 'PI No', renderCell: (row) => row.piNumber || row.pINumber || '-' },
      {
        field: 'date',
        headerName: 'Date',
        renderCell: (row) => new Date(row.date).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' }),
      },
      { field: 'customerName', headerName: 'Customer' },
      { field: 'quotationNumber', headerName: 'Quotation' },
      {
        field: 'grandTotal',
        headerName: 'Grand Total',
        renderCell: (row) => formatCurrency(row.grandTotal),
      },
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
                  navigate(`/sales/proforma-invoices/${row.id}`);
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
        toast.success('Proforma invoice deleted');
        setDeleteTarget(null);
      },
      onError: () => toast.error('Failed to delete PI'),
    });
  };

  const rows = data?.data?.items ?? [];
  const totalCount = data?.data?.totalCount ?? 0;
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch && !statusFilter && !customerFilter;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Proforma Invoices" />
        <EmptyState
          title="No proforma invoices yet"
          description="Create your first PI from approved quotation details."
          action={
            <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/sales/proforma-invoices/new')}>
              New PI
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Proforma Invoices"
        action={
          <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/sales/proforma-invoices/new')}>
            New PI
          </Button>
        }
      />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search PI…"
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
        onRowClick={(row) => navigate(`/sales/proforma-invoices/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Proforma Invoice"
        message={`Are you sure you want to delete "${deleteTarget?.piNumber || deleteTarget?.pINumber}"? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
