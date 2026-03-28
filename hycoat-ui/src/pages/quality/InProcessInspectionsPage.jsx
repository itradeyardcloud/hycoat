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
import { Add, Search, Edit, Delete, CheckCircle, Cancel } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { useInProcessInspections, useDeleteInProcessInspection } from '@/hooks/useInProcessInspections';
import { useProductionWorkOrderLookup } from '@/hooks/useProductionWorkOrders';
import useDebounce from '@/hooks/useDebounce';

export default function InProcessInspectionsPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [pwoFilter, setPwoFilter] = useState(null);
  const [dateFilter, setDateFilter] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useInProcessInspections({
    search: debouncedSearch || undefined,
    productionWorkOrderId: pwoFilter?.id || undefined,
    date: dateFilter || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const { data: pwos } = useProductionWorkOrderLookup();
  const deleteMutation = useDeleteInProcessInspection();

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
      {
        field: 'time',
        headerName: 'Time',
        renderCell: (row) => {
          if (!row.time) return '—';
          const parts = row.time.split(':');
          return `${parts[0]}:${parts[1]}`;
        },
      },
      { field: 'pwoNumber', headerName: 'PWO Number' },
      { field: 'customerName', headerName: 'Customer' },
      {
        field: 'dftAvg',
        headerName: 'DFT Avg (µ)',
        renderCell: (row) => (row.dftAvg != null ? row.dftAvg.toFixed(1) : '—'),
      },
      {
        field: 'tests',
        headerName: 'Tests',
        renderCell: (row) =>
          row.testCount > 0 ? (
            <Stack direction="row" spacing={0.5} alignItems="center">
              <Chip
                icon={<CheckCircle fontSize="small" />}
                label={row.testPassCount}
                size="small"
                color="success"
                variant="outlined"
              />
              {row.testFailCount > 0 && (
                <Chip
                  icon={<Cancel fontSize="small" />}
                  label={row.testFailCount}
                  size="small"
                  color="error"
                  variant="outlined"
                />
              )}
            </Stack>
          ) : (
            '—'
          ),
      },
      {
        field: 'allWithinSpec',
        headerName: 'Status',
        renderCell: (row) => (
          <Chip
            label={row.allWithinSpec ? 'OK' : 'Out of Spec'}
            size="small"
            color={row.allWithinSpec ? 'success' : 'error'}
          />
        ),
      },
      { field: 'inspectorName', headerName: 'Inspector', renderCell: (row) => row.inspectorName ?? '—' },
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
                  navigate(`/quality/in-process/${row.id}`);
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
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch && !pwoFilter && !dateFilter;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="In-Process Inspections" />
        <EmptyState
          title="No inspections yet"
          description="Create your first in-process inspection."
          action={
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => navigate('/quality/in-process/new')}
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
        title="In-Process Inspections"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/quality/in-process/new')}
          >
            New Inspection
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
        onRowClick={(row) => navigate(`/quality/in-process/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Inspection"
        message="Are you sure you want to delete this inspection? This action cannot be undone."
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
