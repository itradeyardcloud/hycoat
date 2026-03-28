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
import { Add, Search, Edit, Delete, PhotoCamera } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { useMaterialInwards, useDeleteMaterialInward } from '@/hooks/useMaterialInwards';
import { useCustomerLookup } from '@/hooks/useCustomers';
import useDebounce from '@/hooks/useDebounce';

const STATUS_CONFIG = {
  Received: { label: 'Received', color: 'info' },
  InspectionPending: { label: 'Insp. Pending', color: 'warning' },
  Inspected: { label: 'Inspected', color: 'primary' },
  Stored: { label: 'Stored', color: 'success' },
};

function StatusChip({ status }) {
  const config = STATUS_CONFIG[status] || { label: status, color: 'default' };
  return <Chip label={config.label} color={config.color} size="small" />;
}

export default function MaterialInwardsPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [statusFilter, setStatusFilter] = useState('');
  const [customerFilter, setCustomerFilter] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useMaterialInwards({
    search: debouncedSearch || undefined,
    status: statusFilter || undefined,
    customerId: customerFilter?.id || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const { data: customers } = useCustomerLookup();
  const deleteMutation = useDeleteMaterialInward();

  const customerOptions = customers?.data ?? [];

  const columns = useMemo(
    () => [
      { field: 'inwardNumber', headerName: 'Inward No' },
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
      { field: 'customerName', headerName: 'Customer' },
      { field: 'woNumber', headerName: 'WO Number' },
      { field: 'vehicleNumber', headerName: 'Vehicle No' },
      {
        field: 'status',
        headerName: 'Status',
        renderCell: (row) => <StatusChip status={row.status} />,
      },
      {
        field: 'hasPhotos',
        headerName: '',
        sortable: false,
        renderCell: (row) =>
          row.hasPhotos ? (
            <Tooltip title="Has photos">
              <PhotoCamera fontSize="small" color="action" />
            </Tooltip>
          ) : null,
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
                  navigate(`/material-inward/inwards/${row.id}`);
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
        toast.success('Material Inward deleted');
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
  const isEmpty =
    !isLoading && rows.length === 0 && !debouncedSearch && !statusFilter && !customerFilter;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Material Inwards" />
        <EmptyState
          title="No material inwards yet"
          description="Create your first material inward to start tracking received material."
          action={
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => navigate('/material-inward/inwards/new')}
            >
              New Material Inward
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Material Inwards"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/material-inward/inwards/new')}
          >
            New Material Inward
          </Button>
        }
      />

      {/* Filters */}
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search inwards…"
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
        onRowClick={(row) => navigate(`/material-inward/inwards/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Material Inward"
        message={`Are you sure you want to delete "${deleteTarget?.inwardNumber}"? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
