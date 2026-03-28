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
import { Add, Search, Edit, Delete } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import StatusChip from '@/components/common/StatusChip';
import { useProductionWorkOrders, useDeleteProductionWorkOrder } from '@/hooks/useProductionWorkOrders';
import useDebounce from '@/hooks/useDebounce';

const STATUS_OPTIONS = ['Created', 'Scheduled', 'InProgress', 'Complete'];

export default function ProductionWorkOrdersPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [statusFilter, setStatusFilter] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useProductionWorkOrders({
    search: debouncedSearch || undefined,
    status: statusFilter || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const deleteMutation = useDeleteProductionWorkOrder();

  const columns = useMemo(
    () => [
      { field: 'pwoNumber', headerName: 'PWO Number' },
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
      { field: 'woNumber', headerName: 'WO Number' },
      { field: 'customerName', headerName: 'Customer' },
      {
        field: 'colorName',
        headerName: 'Powder Color',
        renderCell: (row) =>
          row.colorName ? `${row.powderCode || ''} ${row.colorName}`.trim() : '—',
      },
      { field: 'productionUnitName', headerName: 'Unit' },
      { field: 'shiftAllocation', headerName: 'Shift' },
      {
        field: 'totalTimeHrs',
        headerName: 'Hours',
        renderCell: (row) => (row.totalTimeHrs != null ? `${row.totalTimeHrs}h` : '—'),
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
            <Tooltip title="Edit">
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/ppc/work-orders/${row.id}`);
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
        toast.success('Production Work Order deleted');
        setDeleteTarget(null);
      },
      onError: (err) => {
        const msg = err.response?.data?.message || 'Failed to delete';
        toast.error(msg);
      },
    });
  };

  const rows = data?.data?.items ?? [];
  const totalCount = data?.data?.totalCount ?? 0;
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch && !statusFilter;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Production Work Orders" />
        <EmptyState
          title="No production work orders yet"
          description="Create a new PWO once material is received and work orders are confirmed."
          action={
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => navigate('/ppc/work-orders/new')}
            >
              New PWO
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Production Work Orders"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/ppc/work-orders/new')}
          >
            New PWO
          </Button>
        }
      />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search PWOs…"
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
          {STATUS_OPTIONS.map((s) => (
            <MenuItem key={s} value={s}>
              {s}
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
        onRowClick={(row) => navigate(`/ppc/work-orders/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Production Work Order"
        message={`Are you sure you want to delete "${deleteTarget?.pwoNumber}"? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
