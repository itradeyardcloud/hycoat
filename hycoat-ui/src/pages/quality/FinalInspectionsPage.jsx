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
import { Add, Search, Edit, Delete, Description } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { useFinalInspections, useDeleteFinalInspection } from '@/hooks/useFinalInspections';
import { useProductionWorkOrderLookup } from '@/hooks/useProductionWorkOrders';
import useDebounce from '@/hooks/useDebounce';

const STATUS_COLOR = {
  Approved: 'success',
  Rejected: 'error',
  Rework: 'warning',
};

export default function FinalInspectionsPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [pwoFilter, setPwoFilter] = useState(null);
  const [dateFilter, setDateFilter] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useFinalInspections({
    search: debouncedSearch || undefined,
    productionWorkOrderId: pwoFilter?.id || undefined,
    date: dateFilter || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const { data: pwos } = useProductionWorkOrderLookup();
  const deleteMutation = useDeleteFinalInspection();

  const pwoOptions = pwos?.data ?? [];

  const columns = useMemo(
    () => [
      { field: 'inspectionNumber', headerName: 'Inspection #' },
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
      { field: 'pwoNumber', headerName: 'PWO Number' },
      { field: 'customerName', headerName: 'Customer' },
      { field: 'lotQuantity', headerName: 'Lot Qty' },
      { field: 'sampledQuantity', headerName: 'Sampled' },
      {
        field: 'visualCheckStatus',
        headerName: 'Visual',
        renderCell: (row) =>
          row.visualCheckStatus ? (
            <Chip label={row.visualCheckStatus} size="small" color={row.visualCheckStatus === 'Pass' ? 'success' : 'error'} variant="outlined" />
          ) : '—',
      },
      {
        field: 'dftRecheckStatus',
        headerName: 'DFT',
        renderCell: (row) =>
          row.dftRecheckStatus ? (
            <Chip label={row.dftRecheckStatus} size="small" color={row.dftRecheckStatus === 'Pass' ? 'success' : 'error'} variant="outlined" />
          ) : '—',
      },
      {
        field: 'shadeMatchFinalStatus',
        headerName: 'Shade',
        renderCell: (row) =>
          row.shadeMatchFinalStatus ? (
            <Chip label={row.shadeMatchFinalStatus} size="small" color={row.shadeMatchFinalStatus === 'Pass' ? 'success' : 'error'} variant="outlined" />
          ) : '—',
      },
      {
        field: 'overallStatus',
        headerName: 'Overall',
        renderCell: (row) => (
          <Chip
            label={row.overallStatus}
            size="small"
            color={STATUS_COLOR[row.overallStatus] || 'default'}
          />
        ),
      },
      {
        field: 'actions',
        headerName: '',
        sortable: false,
        renderCell: (row) => (
          <Box sx={{ display: 'flex', gap: 0.5 }}>
            {row.overallStatus === 'Approved' && !row.hasTestCertificate && (
              <Tooltip title="Issue Test Certificate">
                <IconButton
                  size="small"
                  color="primary"
                  onClick={(e) => {
                    e.stopPropagation();
                    navigate(`/quality/test-certificates/new?finalInspectionId=${row.id}`);
                  }}
                >
                  <Description fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
            <Tooltip title="Edit">
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/quality/final/${row.id}`);
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
        <PageHeader title="Final Inspections" />
        <EmptyState
          title="No final inspections yet"
          description="Create your first final inspection report."
          action={
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => navigate('/quality/final/new')}
            >
              New Final Inspection
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Final Inspections"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/quality/final/new')}
          >
            New Final Inspection
          </Button>
        }
      />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search by PWO / Customer / FIR#…"
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
        onRowClick={(row) => navigate(`/quality/final/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Final Inspection"
        message="Are you sure you want to delete this final inspection? This action cannot be undone."
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
