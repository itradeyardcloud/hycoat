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
} from '@mui/material';
import { Add, Search, Edit, Delete } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { usePackingLists, useDeletePackingList } from '@/hooks/usePackingLists';
import useDebounce from '@/hooks/useDebounce';
import { formatDate } from '@/utils/formatters';

export default function PackingListsPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = usePackingLists({
    search: debouncedSearch || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const deleteMutation = useDeletePackingList();

  const columns = useMemo(
    () => [
      {
        field: 'date',
        headerName: 'Date',
        renderCell: (row) => formatDate(row.date),
      },
      { field: 'workOrderNumber', headerName: 'Work Order' },
      { field: 'pwoNumber', headerName: 'PWO #' },
      { field: 'customerName', headerName: 'Customer' },
      { field: 'lineCount', headerName: 'Lines' },
      { field: 'totalQuantity', headerName: 'Total Qty' },
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
                  navigate(`/dispatch/packing-lists/${row.id}`);
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
        toast.success('Packing list deleted');
        setDeleteTarget(null);
      },
      onError: (err) => {
        toast.error(err.response?.data?.message || 'Failed to delete');
      },
    });
  };

  const rows = data?.data?.items ?? [];
  const totalCount = data?.data?.totalCount ?? 0;
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Packing Lists" />
        <EmptyState
          title="No packing lists yet"
          description="Create your first packing list."
          action={
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => navigate('/dispatch/packing-lists/new')}
            >
              New Packing List
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Packing Lists"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/dispatch/packing-lists/new')}
          >
            New Packing List
          </Button>
        }
      />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search by WO / Customer…"
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
        onRowClick={(row) => navigate(`/dispatch/packing-lists/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Packing List"
        message="Are you sure you want to delete this packing list? This action cannot be undone."
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
