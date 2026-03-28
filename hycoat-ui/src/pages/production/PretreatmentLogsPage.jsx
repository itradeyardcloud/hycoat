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
  Autocomplete,
} from '@mui/material';
import { Add, Search, Edit, Delete } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { usePretreatmentLogs, useDeletePretreatmentLog } from '@/hooks/usePretreatmentLogs';
import { useProductionWorkOrderLookup } from '@/hooks/useProductionWorkOrders';
import useDebounce from '@/hooks/useDebounce';

export default function PretreatmentLogsPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [shiftFilter, setShiftFilter] = useState('');
  const [pwoFilter, setPwoFilter] = useState(null);
  const [dateFilter, setDateFilter] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = usePretreatmentLogs({
    search: debouncedSearch || undefined,
    shift: shiftFilter || undefined,
    productionWorkOrderId: pwoFilter?.id || undefined,
    date: dateFilter || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const { data: pwos } = useProductionWorkOrderLookup({ status: 'InProgress' });
  const deleteMutation = useDeletePretreatmentLog();

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
      { field: 'basketNumber', headerName: 'Basket #' },
      {
        field: 'etchTimeMins',
        headerName: 'Etch Time (min)',
        renderCell: (row) => row.etchTimeMins ?? '—',
      },
      { field: 'operatorName', headerName: 'Operator', renderCell: (row) => row.operatorName ?? '—' },
      { field: 'qaSignOffName', headerName: 'QA', renderCell: (row) => row.qaSignOffName ?? '—' },
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
                  navigate(`/production/pretreatment/${row.id}`);
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
        toast.success('Pretreatment log deleted');
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
        <PageHeader title="Pretreatment Logs" />
        <EmptyState
          title="No pretreatment logs yet"
          description="Create your first pretreatment log entry."
          action={
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => navigate('/production/pretreatment/new')}
            >
              New Pretreatment Log
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Pretreatment Logs"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/production/pretreatment/new')}
          >
            New Pretreatment Log
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
        onRowClick={(row) => navigate(`/production/pretreatment/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Pretreatment Log"
        message={`Are you sure you want to delete log for Basket #${deleteTarget?.basketNumber}? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
