import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button, TextField, InputAdornment, IconButton, Box, Tooltip } from '@mui/material';
import { Add, Search, Edit, Delete } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { useCustomers, useDeleteCustomer } from '@/hooks/useCustomers';
import useDebounce from '@/hooks/useDebounce';

export default function CustomersPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useCustomers({
    search: debouncedSearch || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
  });
  const deleteMutation = useDeleteCustomer();

  const columns = useMemo(
    () => [
      { field: 'name', headerName: 'Name' },
      { field: 'city', headerName: 'City' },
      { field: 'gstin', headerName: 'GSTIN' },
      { field: 'contactPerson', headerName: 'Contact' },
      { field: 'phone', headerName: 'Phone' },
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
                  navigate(`/masters/customers/${row.id}/edit`);
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
        toast.success('Customer deleted');
        setDeleteTarget(null);
      },
      onError: () => toast.error('Failed to delete customer'),
    });
  };

  const listData = data?.data ?? data;
  const rows = listData?.items ?? [];
  const totalCount = listData?.totalCount ?? 0;
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Customers" />
        <EmptyState
          title="No customers yet"
          description="Add your first customer to get started."
          action={
            <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/masters/customers/new')}>
              Add Customer
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Customers"
        action={
          <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/masters/customers/new')}>
            Add Customer
          </Button>
        }
      />

      <TextField
        placeholder="Search customers…"
        size="small"
        value={search}
        onChange={(e) => {
          setSearch(e.target.value);
          setPage(0);
        }}
        sx={{ mb: 2, maxWidth: 400 }}
        fullWidth
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
        onRowClick={(row) => navigate(`/masters/customers/${row.id}/edit`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Customer"
        message={`Are you sure you want to delete "${deleteTarget?.name}"? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
