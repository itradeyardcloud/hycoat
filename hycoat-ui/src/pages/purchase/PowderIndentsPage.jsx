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
} from '@mui/material';
import { Add, Search, Edit, Delete, ShoppingCart } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { usePowderIndents, useDeletePowderIndent, useUpdatePowderIndentStatus } from '@/hooks/usePowderIndents';
import useDebounce from '@/hooks/useDebounce';

const STATUS_COLOR = {
  Requested: 'warning',
  Approved: 'info',
  Ordered: 'secondary',
  Received: 'success',
};

export default function PowderIndentsPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [dateFilter, setDateFilter] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = usePowderIndents({
    search: debouncedSearch || undefined,
    date: dateFilter || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const deleteMutation = useDeletePowderIndent();
  const statusMutation = useUpdatePowderIndentStatus();

  const handleApprove = (row) => {
    statusMutation.mutate(
      { id: row.id, data: { status: 'Approved' } },
      {
        onSuccess: () => toast.success('Indent approved'),
        onError: (err) => toast.error(err.response?.data?.message || 'Failed to approve'),
      },
    );
  };

  const columns = useMemo(
    () => [
      { field: 'indentNumber', headerName: 'Indent #' },
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
      { field: 'pwoNumber', headerName: 'PWO #' },
      { field: 'requestedByName', headerName: 'Requested By' },
      { field: 'lineCount', headerName: 'Lines' },
      {
        field: 'totalQtyKg',
        headerName: 'Total Qty (kg)',
        renderCell: (row) => row.totalQtyKg?.toFixed(2),
      },
      {
        field: 'status',
        headerName: 'Status',
        renderCell: (row) => (
          <Chip
            label={row.status}
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
            {row.status === 'Requested' && (
              <Tooltip title="Approve">
                <IconButton
                  size="small"
                  color="success"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleApprove(row);
                  }}
                >
                  <ShoppingCart fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
            {row.status === 'Approved' && (
              <Tooltip title="Create PO">
                <IconButton
                  size="small"
                  color="primary"
                  onClick={(e) => {
                    e.stopPropagation();
                    navigate(`/purchase/orders/new?indentId=${row.id}`);
                  }}
                >
                  <ShoppingCart fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
            <Tooltip title="Edit">
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/purchase/indents/${row.id}`);
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
    [navigate, statusMutation],
  );

  const handleDelete = () => {
    deleteMutation.mutate(deleteTarget.id, {
      onSuccess: () => {
        toast.success('Indent deleted');
        setDeleteTarget(null);
      },
      onError: (err) => {
        toast.error(err.response?.data?.message || 'Failed to delete');
      },
    });
  };

  const rows = data?.data?.items ?? [];
  const totalCount = data?.data?.totalCount ?? 0;
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch && !dateFilter;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Powder Indents" />
        <EmptyState
          title="No powder indents yet"
          description="Create your first powder indent request."
          action={
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => navigate('/purchase/indents/new')}
            >
              New Indent
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Powder Indents"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/purchase/indents/new')}
          >
            New Indent
          </Button>
        }
      />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search by indent # / PWO…"
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
        onRowClick={(row) => navigate(`/purchase/indents/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Powder Indent"
        message="Are you sure you want to delete this indent? This action cannot be undone."
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
