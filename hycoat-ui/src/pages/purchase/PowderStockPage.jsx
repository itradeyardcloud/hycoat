import { useState, useMemo, useCallback } from 'react';
import {
  TextField,
  Box,
  Chip,
  Stack,
  Switch,
  FormControlLabel,
} from '@mui/material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import EmptyState from '@/components/common/EmptyState';
import { usePowderStock, useLowStock, useUpdateReorderLevel } from '@/hooks/usePowderStock';

export default function PowderStockPage() {
  const [lowStockOnly, setLowStockOnly] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [editValue, setEditValue] = useState('');

  const { data: allData, isLoading: loadingAll } = usePowderStock();
  const { data: lowData, isLoading: loadingLow } = useLowStock();
  const reorderMutation = useUpdateReorderLevel();

  const data = lowStockOnly ? lowData : allData;
  const isLoading = lowStockOnly ? loadingLow : loadingAll;

  const handleReorderSave = useCallback(
    (powderColorId) => {
      const val = parseFloat(editValue);
      if (isNaN(val) || val < 0) {
        toast.error('Enter a valid reorder level');
        return;
      }
      reorderMutation.mutate(
        { powderColorId, data: { reorderLevelKg: val } },
        {
          onSuccess: () => {
            toast.success('Reorder level updated');
            setEditingId(null);
          },
          onError: (err) => toast.error(err.response?.data?.message || 'Failed to update'),
        },
      );
    },
    [editValue, reorderMutation],
  );

  const columns = useMemo(
    () => [
      { field: 'powderCode', headerName: 'Powder Code' },
      { field: 'colorName', headerName: 'Color' },
      { field: 'ralCode', headerName: 'RAL Code' },
      {
        field: 'currentStockKg',
        headerName: 'Current Stock (kg)',
        renderCell: (row) => row.currentStockKg?.toFixed(2),
      },
      {
        field: 'reorderLevelKg',
        headerName: 'Reorder Level (kg)',
        renderCell: (row) => {
          if (editingId === row.powderColorId) {
            return (
              <TextField
                size="small"
                type="number"
                value={editValue}
                onChange={(e) => setEditValue(e.target.value)}
                onBlur={() => handleReorderSave(row.powderColorId)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') handleReorderSave(row.powderColorId);
                  if (e.key === 'Escape') setEditingId(null);
                }}
                autoFocus
                sx={{ width: 100 }}
              />
            );
          }
          return (
            <Box
              sx={{ cursor: 'pointer', '&:hover': { textDecoration: 'underline' } }}
              onClick={(e) => {
                e.stopPropagation();
                setEditingId(row.powderColorId);
                setEditValue(row.reorderLevelKg?.toString() ?? '0');
              }}
            >
              {row.reorderLevelKg?.toFixed(2) ?? '0.00'}
            </Box>
          );
        },
      },
      {
        field: 'status',
        headerName: 'Status',
        renderCell: (row) =>
          row.isBelowReorderLevel ? (
            <Chip label="Low Stock" size="small" color="error" />
          ) : (
            <Chip label="OK" size="small" color="success" variant="outlined" />
          ),
      },
    ],
    [editingId, editValue, handleReorderSave],
  );

  const rows = data?.data ?? [];
  const isEmpty = !isLoading && rows.length === 0;

  if (isEmpty && !lowStockOnly) {
    return (
      <>
        <PageHeader title="Powder Stock" />
        <EmptyState
          title="No stock data yet"
          description="Stock levels are automatically updated when GRNs are created."
        />
      </>
    );
  }

  return (
    <>
      <PageHeader title="Powder Stock" />

      <Stack direction="row" spacing={2} sx={{ mb: 2, alignItems: 'center' }}>
        <FormControlLabel
          control={
            <Switch
              checked={lowStockOnly}
              onChange={(e) => setLowStockOnly(e.target.checked)}
            />
          }
          label="Low Stock Only"
        />
      </Stack>

      <DataTable
        columns={columns}
        rows={rows}
        loading={isLoading}
        page={0}
        rowsPerPage={rows.length || 20}
        totalCount={rows.length}
        onPageChange={() => {}}
        onRowsPerPageChange={() => {}}
      />
    </>
  );
}
