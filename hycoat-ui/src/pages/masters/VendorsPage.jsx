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
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Stack,
} from '@mui/material';
import { Add, Search, Edit, Delete } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { useVendors, useDeleteVendor } from '@/hooks/useVendors';
import useDebounce from '@/hooks/useDebounce';

const VENDOR_TYPES = ['Powder', 'Chemical', 'Consumable', 'Other'];

export default function VendorsPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [vendorType, setVendorType] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useVendors({
    search: debouncedSearch || undefined,
    vendorType: vendorType || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
  });
  const deleteMutation = useDeleteVendor();

  const columns = useMemo(
    () => [
      { field: 'name', headerName: 'Name' },
      {
        field: 'vendorType',
        headerName: 'Type',
        renderCell: (row) => row.vendorType ? <Chip label={row.vendorType} size="small" variant="outlined" /> : '—',
      },
      { field: 'city', headerName: 'City' },
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
                  navigate(`/masters/vendors/${row.id}/edit`);
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
        toast.success('Vendor deleted');
        setDeleteTarget(null);
      },
      onError: () => toast.error('Failed to delete vendor'),
    });
  };

  const rows = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch && !vendorType;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Vendors" />
        <EmptyState
          title="No vendors yet"
          description="Add your first vendor to get started."
          action={
            <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/masters/vendors/new')}>
              Add Vendor
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Vendors"
        action={
          <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/masters/vendors/new')}>
            Add Vendor
          </Button>
        }
      />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search vendors…"
          size="small"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(0);
          }}
          sx={{ maxWidth: 400 }}
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
        <FormControl size="small" sx={{ minWidth: 180 }}>
          <InputLabel>Vendor Type</InputLabel>
          <Select
            value={vendorType}
            label="Vendor Type"
            onChange={(e) => {
              setVendorType(e.target.value);
              setPage(0);
            }}
          >
            <MenuItem value="">All Types</MenuItem>
            {VENDOR_TYPES.map((t) => (
              <MenuItem key={t} value={t}>
                {t}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
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
        onRowClick={(row) => navigate(`/masters/vendors/${row.id}/edit`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Vendor"
        message={`Are you sure you want to delete "${deleteTarget?.name}"?`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
