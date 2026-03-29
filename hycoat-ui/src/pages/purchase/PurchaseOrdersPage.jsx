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
  Autocomplete,
} from '@mui/material';
import { Add, Search, Edit, Delete, PictureAsPdf } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { usePurchaseOrders, useDeletePurchaseOrder } from '@/hooks/usePurchaseOrders';
import { purchaseOrderService } from '@/services/purchaseService';
import { useVendorLookup } from '@/hooks/useVendors';
import useDebounce from '@/hooks/useDebounce';

const STATUS_COLOR = {
  Draft: 'default',
  Sent: 'info',
  PartiallyReceived: 'warning',
  Received: 'success',
  Cancelled: 'error',
};

export default function PurchaseOrdersPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [vendorFilter, setVendorFilter] = useState(null);
  const [dateFilter, setDateFilter] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = usePurchaseOrders({
    search: debouncedSearch || undefined,
    vendorId: vendorFilter?.id || undefined,
    date: dateFilter || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const { data: vendors } = useVendorLookup();
  const deleteMutation = useDeletePurchaseOrder();

  const vendorOptions = vendors?.data ?? [];

  const handleDownloadPdf = async (row) => {
    try {
      const response = await purchaseOrderService.getPdf(row.id);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `${row.poNumber}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch {
      toast.error('Failed to download PDF');
    }
  };

  const columns = useMemo(
    () => [
      { field: 'poNumber', headerName: 'PO #' },
      {
        field: 'date',
        headerName: 'Date',
        renderCell: (row) =>
          new Date(row.date).toLocaleDateString('en-IN', {
            day: '2-digit',
            month: 'short',
            year: 'numeric',
          }),
      },
      { field: 'vendorName', headerName: 'Vendor' },
      { field: 'indentNumber', headerName: 'Indent #' },
      {
        field: 'totalQtyKg',
        headerName: 'Qty (kg)',
        renderCell: (row) => row.totalQtyKg?.toFixed(2),
      },
      {
        field: 'totalAmount',
        headerName: 'Amount (₹)',
        renderCell: (row) => row.totalAmount?.toLocaleString('en-IN', { maximumFractionDigits: 2 }),
      },
      {
        field: 'status',
        headerName: 'Status',
        renderCell: (row) => (
          <Chip
            label={row.status === 'PartiallyReceived' ? 'Partial' : row.status}
            size="small"
            color={STATUS_COLOR[row.status] || 'default'}
          />
        ),
      },
      {
        field: 'actions',
        headerName: '',
        sortable: false,
        renderCell: (row) => (
          <Box sx={{ display: 'flex', gap: 0.5 }}>
            <Tooltip title="Download PDF">
              <IconButton
                size="small"
                color="primary"
                onClick={(e) => {
                  e.stopPropagation();
                  handleDownloadPdf(row);
                }}
              >
                <PictureAsPdf fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Edit">
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/purchase/orders/${row.id}`);
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
        toast.success('Purchase order deleted');
        setDeleteTarget(null);
      },
      onError: (err) => {
        toast.error(err.response?.data?.message || 'Failed to delete');
      },
    });
  };

  const rows = data?.data?.items ?? [];
  const totalCount = data?.data?.totalCount ?? 0;
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch && !vendorFilter && !dateFilter;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Purchase Orders" />
        <EmptyState
          title="No purchase orders yet"
          description="Create your first purchase order."
          action={
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => navigate('/purchase/orders/new')}
            >
              New Purchase Order
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Purchase Orders"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/purchase/orders/new')}
          >
            New Purchase Order
          </Button>
        }
      />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search by PO # / Vendor…"
          size="small"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(0);
          }}
          sx={{ minWidth: 260 }}
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
          type="date"
          size="small"
          label="Date"
          value={dateFilter}
          onChange={(e) => {
            setDateFilter(e.target.value);
            setPage(0);
          }}
          sx={{ minWidth: 160 }}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <Autocomplete
          size="small"
          options={vendorOptions}
          getOptionLabel={(o) => o.name}
          value={vendorFilter}
          onChange={(_, val) => {
            setVendorFilter(val);
            setPage(0);
          }}
          renderInput={(params) => <TextField {...params} label="Vendor" />}
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
        onRowClick={(row) => navigate(`/purchase/orders/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Purchase Order"
        message="Are you sure you want to delete this purchase order? This action cannot be undone."
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
