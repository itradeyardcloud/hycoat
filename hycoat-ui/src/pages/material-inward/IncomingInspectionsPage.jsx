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
} from '@mui/material';
import { Add, Search, Edit, Delete } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { useIncomingInspections, useDeleteIncomingInspection } from '@/hooks/useIncomingInspections';
import useDebounce from '@/hooks/useDebounce';

const STATUS_CONFIG = {
  Pass: { label: 'Pass', color: 'success' },
  Fail: { label: 'Fail', color: 'error' },
  Conditional: { label: 'Conditional', color: 'warning' },
};

function StatusChip({ status }) {
  const config = STATUS_CONFIG[status] || { label: status, color: 'default' };
  return <Chip label={config.label} color={config.color} size="small" />;
}

export default function IncomingInspectionsPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [statusFilter, setStatusFilter] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useIncomingInspections({
    search: debouncedSearch || undefined,
    overallStatus: statusFilter || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const deleteMutation = useDeleteIncomingInspection();

  const columns = useMemo(
    () => [
      { field: 'inspectionNumber', headerName: 'Inspection No' },
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
      { field: 'inwardNumber', headerName: 'Inward No' },
      { field: 'customerName', headerName: 'Customer' },
      {
        field: 'overallStatus',
        headerName: 'Status',
        renderCell: (row) => <StatusChip status={row.overallStatus} />,
      },
      { field: 'inspectedByName', headerName: 'Inspector' },
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
                  navigate(`/material-inward/inspections/${row.id}`);
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
        toast.success('Inspection deleted');
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
        <PageHeader title="Incoming Inspections" />
        <EmptyState
          title="No inspections yet"
          description="Create your first incoming inspection to start quality checks."
          action={
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => navigate('/material-inward/inspections/new')}
            >
              New Inspection
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Incoming Inspections"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/material-inward/inspections/new')}
          >
            New Inspection
          </Button>
        }
      />

      {/* Filters */}
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search inspections…"
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
        onRowClick={(row) => navigate(`/material-inward/inspections/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Inspection"
        message={`Are you sure you want to delete "${deleteTarget?.inspectionNumber}"? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
