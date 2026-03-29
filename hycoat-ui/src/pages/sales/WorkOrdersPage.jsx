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
  Card,
  CardActionArea,
  CardContent,
  Typography,
} from '@mui/material';
import { Add, Search, Edit, Delete, Visibility } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { useWorkOrders, useDeleteWorkOrder, useWorkOrderStats } from '@/hooks/useWorkOrders';
import { useCustomerLookup } from '@/hooks/useCustomers';
import { useProcessTypes } from '@/hooks/useProcessTypes';
import { usePowderColorLookup } from '@/hooks/usePowderColors';
import useDebounce from '@/hooks/useDebounce';

const STATUS_CONFIG = {
  Created: { label: 'Created', color: 'default' },
  MaterialAwaited: { label: 'Material Awaited', color: 'warning' },
  MaterialReceived: { label: 'Material Received', color: 'info' },
  InProduction: { label: 'In Production', color: 'primary' },
  QAComplete: { label: 'QA Complete', color: 'secondary' },
  Dispatched: { label: 'Dispatched', color: 'success' },
  Invoiced: { label: 'Invoiced', color: 'success' },
  Closed: { label: 'Closed', color: 'default' },
};

function StatusChip({ status }) {
  const config = STATUS_CONFIG[status] || { label: status, color: 'default' };
  return <Chip label={config.label} color={config.color} size="small" />;
}

export default function WorkOrdersPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [statusFilter, setStatusFilter] = useState('');
  const [customerFilter, setCustomerFilter] = useState(null);
  const [processTypeFilter, setProcessTypeFilter] = useState(null);
  const [powderColorFilter, setPowderColorFilter] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useWorkOrders({
    search: debouncedSearch || undefined,
    status: statusFilter || undefined,
    customerId: customerFilter?.id || undefined,
    processTypeId: processTypeFilter?.id || undefined,
    powderColorId: powderColorFilter?.id || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });

  const { data: stats } = useWorkOrderStats();
  const { data: customers } = useCustomerLookup();
  const { data: processTypes } = useProcessTypes();
  const { data: powderColors } = usePowderColorLookup();
  const deleteMutation = useDeleteWorkOrder();

  const statsData = stats?.data;
  const customerOptions = customers?.data ?? [];
  const processTypeOptions = processTypes?.data ?? [];
  const powderColorOptions = powderColors?.data ?? [];

  const columns = useMemo(
    () => [
      { field: 'woNumber', headerName: 'WO No', renderCell: (row) => row.woNumber || row.wONumber },
      {
        field: 'date',
        headerName: 'Date',
        renderCell: (row) => new Date(row.date).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' }),
      },
      { field: 'customerName', headerName: 'Customer' },
      { field: 'projectName', headerName: 'Project' },
      { field: 'processTypeName', headerName: 'Process' },
      { field: 'powderColorName', headerName: 'Powder Color' },
      {
        field: 'status',
        headerName: 'Status',
        renderCell: (row) => <StatusChip status={row.status} />,
      },
      {
        field: 'actions',
        headerName: '',
        sortable: false,
        renderCell: (row) => (
          <Box sx={{ display: 'flex', gap: 0.5 }}>
            <Tooltip title="View">
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/sales/work-orders/${row.id}`);
                }}
              >
                <Visibility fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Edit">
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/sales/work-orders/${row.id}/edit`);
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
        toast.success('Work order deleted');
        setDeleteTarget(null);
      },
      onError: (err) => {
        const msg = err.response?.data?.errors?.[0] || err.response?.data?.message || 'Failed to delete work order';
        toast.error(msg);
      },
    });
  };

  const handleStatusCardClick = (status) => {
    setStatusFilter((prev) => (prev === status ? '' : status));
    setPage(0);
  };

  const rows = data?.data?.items ?? [];
  const totalCount = data?.data?.totalCount ?? 0;
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch && !statusFilter;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Work Orders" />
        <EmptyState
          title="No work orders yet"
          description="Create your first work order to initiate production flow."
          action={
            <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/sales/work-orders/new')}>
              New Work Order
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Work Orders"
        action={
          <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/sales/work-orders/new')}>
            New Work Order
          </Button>
        }
      />

      {statsData && (
        <Stack direction="row" spacing={1.5} sx={{ mb: 2, overflowX: 'auto', pb: 1 }}>
          {Object.entries(STATUS_CONFIG).map(([key, config]) => {
            const count = statsData[key.charAt(0).toLowerCase() + key.slice(1)] ?? 0;
            const isActive = statusFilter === key;
            return (
              <Card
                key={key}
                variant={isActive ? 'elevation' : 'outlined'}
                sx={{
                  minWidth: 120,
                  flexShrink: 0,
                  borderColor: isActive ? `${config.color}.main` : undefined,
                  borderWidth: isActive ? 2 : 1,
                }}
              >
                <CardActionArea onClick={() => handleStatusCardClick(key)}>
                  <CardContent sx={{ py: 1, px: 2, '&:last-child': { pb: 1 } }}>
                    <Typography variant="caption" color="text.secondary">
                      {config.label}
                    </Typography>
                    <Typography variant="h6" fontWeight={700}>
                      {count}
                    </Typography>
                  </CardContent>
                </CardActionArea>
              </Card>
            );
          })}
        </Stack>
      )}

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search work orders…"
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
          select
          size="small"
          label="Status"
          value={statusFilter}
          onChange={(e) => {
            setStatusFilter(e.target.value);
            setPage(0);
          }}
          sx={{ minWidth: 150 }}
        >
          <MenuItem value="">All</MenuItem>
          {Object.entries(STATUS_CONFIG).map(([key, config]) => (
            <MenuItem key={key} value={key}>
              {config.label}
            </MenuItem>
          ))}
        </TextField>

        <Autocomplete
          size="small"
          options={customerOptions}
          getOptionLabel={(o) => o.name}
          value={customerFilter}
          onChange={(_, val) => {
            setCustomerFilter(val);
            setPage(0);
          }}
          renderInput={(params) => <TextField {...params} label="Customer" />}
          sx={{ minWidth: 180 }}
          isOptionEqualToValue={(opt, val) => opt.id === val.id}
        />

        <Autocomplete
          size="small"
          options={processTypeOptions}
          getOptionLabel={(o) => o.name}
          value={processTypeFilter}
          onChange={(_, val) => {
            setProcessTypeFilter(val);
            setPage(0);
          }}
          renderInput={(params) => <TextField {...params} label="Process" />}
          sx={{ minWidth: 180 }}
          isOptionEqualToValue={(opt, val) => opt.id === val.id}
        />

        <Autocomplete
          size="small"
          options={powderColorOptions}
          getOptionLabel={(o) => o.name}
          value={powderColorFilter}
          onChange={(_, val) => {
            setPowderColorFilter(val);
            setPage(0);
          }}
          renderInput={(params) => <TextField {...params} label="Powder Color" />}
          sx={{ minWidth: 180 }}
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
        onRowClick={(row) => navigate(`/sales/work-orders/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Work Order"
        message={`Are you sure you want to delete "${deleteTarget?.woNumber || deleteTarget?.wONumber}"? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
