import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Button,
  TextField,
  InputAdornment,
  IconButton,
  Box,
  Tooltip,
  Stack,
  MenuItem,
} from '@mui/material';
import { Add, Search, Edit, Delete, PictureAsPdf, Email } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import StatusChip from '@/components/common/StatusChip';
import {
  useInvoices,
  useDeleteInvoice,
  useUpdateInvoiceStatus,
  useGenerateInvoicePdf,
} from '@/hooks/useInvoices';
import useDebounce from '@/hooks/useDebounce';
import { formatDate, formatCurrency, formatSFT } from '@/utils/formatters';

const INV_STATUSES = ['', 'Draft', 'Finalized', 'Sent', 'Paid'];

export default function InvoicesPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [statusFilter, setStatusFilter] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useInvoices({
    search: debouncedSearch || undefined,
    status: statusFilter || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const deleteMutation = useDeleteInvoice();
  const statusMutation = useUpdateInvoiceStatus();
  const pdfMutation = useGenerateInvoicePdf();

  const handleDownloadPdf = (row) => {
    pdfMutation.mutate(row.id, {
      onSuccess: (response) => {
        const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
        const link = document.createElement('a');
        link.href = url;
        link.download = `${row.invoiceNumber || 'Invoice'}.pdf`;
        link.click();
        URL.revokeObjectURL(url);
      },
      onError: () => toast.error('Failed to generate PDF'),
    });
  };

  const columns = useMemo(
    () => [
      { field: 'invoiceNumber', headerName: 'Invoice #' },
      {
        field: 'date',
        headerName: 'Date',
        renderCell: (row) => formatDate(row.date),
      },
      { field: 'customerName', headerName: 'Customer' },
      {
        field: 'totalSFT',
        headerName: 'Total SFT',
        renderCell: (row) => formatSFT(row.totalSFT),
      },
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
            {row.status === 'Draft' && (
              <Tooltip title="Edit">
                <IconButton
                  size="small"
                  onClick={(e) => {
                    e.stopPropagation();
                    navigate(`/dispatch/invoices/${row.id}`);
                  }}
                >
                  <Edit fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
            {row.status === 'Draft' && (
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
            )}
          </Box>
        ),
      },
    ],
    [navigate],
  );

  const handleDelete = () => {
    deleteMutation.mutate(deleteTarget.id, {
      onSuccess: () => {
        toast.success('Invoice deleted');
        setDeleteTarget(null);
      },
      onError: (err) => {
        toast.error(err.response?.data?.message || 'Failed to delete');
      },
    });
  };

  const rows = data?.data?.items ?? [];
  const totalCount = data?.data?.totalCount ?? 0;
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch && !statusFilter;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Invoices" />
        <EmptyState
          title="No invoices yet"
          description="Create your first invoice."
          action={
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => navigate('/dispatch/invoices/new')}
            >
              New Invoice
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Invoices"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/dispatch/invoices/new')}
          >
            New Invoice
          </Button>
        }
      />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search by Invoice# / Customer…"
          size="small"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(0);
          }}
          sx={{ minWidth: 280 }}
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
          {INV_STATUSES.map((s) => (
            <MenuItem key={s} value={s}>
              {s || 'All'}
            </MenuItem>
          ))}
        </TextField>
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
        onRowClick={(row) => navigate(`/dispatch/invoices/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Invoice"
        message="Are you sure you want to delete this invoice? This action cannot be undone."
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
