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
import { useProductionLogs, useDeleteProductionLog } from '@/hooks/useProductionLogs';
import { useProductionWorkOrderLookup } from '@/hooks/useProductionWorkOrders';
import useDebounce from '@/hooks/useDebounce';

export default function CoatingLogsPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [shiftFilter, setShiftFilter] = useState('');
  const [pwoFilter, setPwoFilter] = useState(null);
  const [dateFilter, setDateFilter] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useProductionLogs({
    search: debouncedSearch || undefined,
    shift: shiftFilter || undefined,
    productionWorkOrderId: pwoFilter?.id || undefined,
    date: dateFilter || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const { data: pwos } = useProductionWorkOrderLookup({ status: 'InProgress' });
  const deleteMutation = useDeleteProductionLog();

  const pwoOptions = pwos?.data ?? [];

  const columns = useMemo(
    () => [
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
      { field: 'shift', headerName: 'Shift' },
      { field: 'pwoNumber', headerName: 'PWO Number' },
      { field: 'customerName', headerName: 'Customer' },
      {
        field: 'conveyorSpeedMtrPerMin',
        headerName: 'Speed (m/min)',
        renderCell: (row) => row.conveyorSpeedMtrPerMin ?? '—',
      },
      {
        field: 'ovenTemperature',
        headerName: 'Oven Temp (°C)',
        renderCell: (row) => row.ovenTemperature ?? '—',
      },
      {
        field: 'powderBatchNo',
        headerName: 'Powder Batch',
        renderCell: (row) => row.powderBatchNo ?? '—',
      },
      {
        field: 'photoCount',
        headerName: 'Photos',
        renderCell: (row) =>
          row.photoCount > 0 ? (
            <Chip
              icon={<PhotoCamera fontSize="small" />}
              label={row.photoCount}
              size="small"
              variant="outlined"
            />
          ) : (
            '—'
          ),
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
                  navigate(`/production/coating/${row.id}`);
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
        toast.success('Coating log deleted');
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
    !isLoading && rows.length === 0 && !debouncedSearch && !shiftFilter && !pwoFilter && !dateFilter;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Coating Logs" />
        <EmptyState
          title="No coating logs yet"
          description="Create your first coating log entry."
          action={
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => navigate('/production/coating/new')}
            >
              New Coating Log
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Coating Logs"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/production/coating/new')}
          >
            New Coating Log
          </Button>
        }
      />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search by PWO / Customer…"
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
        <TextField
          select
          size="small"
          label="Shift"
          value={shiftFilter}
          onChange={(e) => {
            setShiftFilter(e.target.value);
            setPage(0);
          }}
          sx={{ minWidth: 120 }}
        >
          <MenuItem value="">All</MenuItem>
          <MenuItem value="Day">Day</MenuItem>
          <MenuItem value="Night">Night</MenuItem>
        </TextField>
        <Autocomplete
          size="small"
          options={pwoOptions}
          getOptionLabel={(o) => o.name}
          value={pwoFilter}
          onChange={(_, val) => {
            setPwoFilter(val);
            setPage(0);
          }}
          renderInput={(params) => <TextField {...params} label="PWO" />}
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
        onRowClick={(row) => navigate(`/production/coating/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Coating Log"
        message={`Are you sure you want to delete this coating log? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
