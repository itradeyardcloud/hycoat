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
import { Add, Search, Edit, Delete, PictureAsPdf, LocalShipping } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import StatusChip from '@/components/common/StatusChip';
import {
  useDeliveryChallans,
  useDeleteDeliveryChallan,
  useUpdateDCStatus,
  useDownloadDCPdf,
} from '@/hooks/useDeliveryChallans';
import useDebounce from '@/hooks/useDebounce';
import { formatDate } from '@/utils/formatters';

const DC_STATUSES = ['', 'Created', 'Dispatched', 'Delivered'];

export default function DeliveryChallansPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [statusFilter, setStatusFilter] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useDeliveryChallans({
    search: debouncedSearch || undefined,
    status: statusFilter || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const deleteMutation = useDeleteDeliveryChallan();
  const statusMutation = useUpdateDCStatus();
  const downloadPdfMutation = useDownloadDCPdf();

  const handleDownloadPdf = (row) => {
    downloadPdfMutation.mutate(row.id, {
      onSuccess: (response) => {
        const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
        const link = document.createElement('a');
        link.href = url;
        link.download = `${row.dcNumber || 'DC'}.pdf`;
        link.click();
        URL.revokeObjectURL(url);
      },
      onError: () => toast.error('Failed to download PDF'),
    });
  };

  const handleStatusChange = (id, status) => {
    statusMutation.mutate(
      { id, status },
      {
        onSuccess: () => toast.success(`Status updated to ${status}`),
        onError: (err) => toast.error(err.response?.data?.message || 'Failed to update status'),
      },
    );
  };

  const columns = useMemo(
    () => [
      { field: 'dcNumber', headerName: 'DC #' },
      {
        field: 'date',
        headerName: 'Date',
        renderCell: (row) => formatDate(row.date),
      },
      { field: 'customerName', headerName: 'Customer' },
      { field: 'vehicleNumber', headerName: 'Vehicle' },
      { field: 'lineCount', headerName: 'Lines' },
      { field: 'totalQuantity', headerName: 'Total Qty' },
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
            {row.status === 'Created' && (
              <Tooltip title="Mark Dispatched">
                <IconButton
                  size="small"
                  color="info"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleStatusChange(row.id, 'Dispatched');
                  }}
                >
                  <LocalShipping fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
            <Tooltip title="Edit">
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/dispatch/delivery-challans/${row.id}`);
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
        toast.success('Delivery challan deleted');
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
        <PageHeader title="Delivery Challans" />
        <EmptyState
          title="No delivery challans yet"
          description="Create your first delivery challan."
          action={
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => navigate('/dispatch/delivery-challans/new')}
            >
              New Delivery Challan
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Delivery Challans"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/dispatch/delivery-challans/new')}
          >
            New Delivery Challan
          </Button>
        }
      />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search by DC# / Customer / Vehicle…"
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
          {DC_STATUSES.map((s) => (
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
        onRowClick={(row) => navigate(`/dispatch/delivery-challans/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Delivery Challan"
        message="Are you sure you want to delete this delivery challan? This action cannot be undone."
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
