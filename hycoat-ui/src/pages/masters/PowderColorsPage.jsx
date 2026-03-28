import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Button,
  TextField,
  InputAdornment,
  IconButton,
  Box,
  Tooltip,
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
import { usePowderColors, useDeletePowderColor } from '@/hooks/usePowderColors';
import { useVendorLookup } from '@/hooks/useVendors';
import useDebounce from '@/hooks/useDebounce';

export default function PowderColorsPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [vendorId, setVendorId] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = usePowderColors({
    search: debouncedSearch || undefined,
    vendorId: vendorId || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
  });
  const deleteMutation = useDeletePowderColor();
  const { data: vendors } = useVendorLookup();

  const columns = useMemo(
    () => [
      { field: 'powderCode', headerName: 'Powder Code' },
      { field: 'colorName', headerName: 'Color Name' },
      { field: 'ralCode', headerName: 'RAL Code' },
      { field: 'make', headerName: 'Make' },
      { field: 'vendorName', headerName: 'Vendor' },
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
                  navigate(`/masters/powder-colors/${row.id}/edit`);
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
        toast.success('Powder color deleted');
        setDeleteTarget(null);
      },
      onError: () => toast.error('Failed to delete powder color'),
    });
  };

  const listData = data?.data ?? data;
  const vendorOptions = vendors?.data ?? vendors ?? [];
  const rows = listData?.items ?? [];
  const totalCount = listData?.totalCount ?? 0;
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch && !vendorId;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Powder Colors" />
        <EmptyState
          title="No powder colors yet"
          description="Add your first powder color to get started."
          action={
            <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/masters/powder-colors/new')}>
              Add Powder Color
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Powder Colors"
        action={
          <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/masters/powder-colors/new')}>
            Add Powder Color
          </Button>
        }
      />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search powder colors…"
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
          <InputLabel>Vendor</InputLabel>
          <Select
            value={vendorId}
            label="Vendor"
            onChange={(e) => {
              setVendorId(e.target.value);
              setPage(0);
            }}
          >
            <MenuItem value="">All Vendors</MenuItem>
            {vendorOptions?.map((v) => (
              <MenuItem key={v.id} value={v.id}>
                {v.name}
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
        onRowClick={(row) => navigate(`/masters/powder-colors/${row.id}/edit`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Powder Color"
        message={`Are you sure you want to delete "${deleteTarget?.powderCode} — ${deleteTarget?.colorName}"?`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
