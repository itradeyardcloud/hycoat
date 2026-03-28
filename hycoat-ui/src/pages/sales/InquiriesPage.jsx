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
  Card,
  CardActionArea,
  CardContent,
  Typography,
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
import { useInquiries, useInquiryStats, useDeleteInquiry } from '@/hooks/useInquiries';
import { useCustomerLookup } from '@/hooks/useCustomers';
import useDebounce from '@/hooks/useDebounce';

const STATUS_CONFIG = {
  New: { label: 'New', color: 'info' },
  QuotationSent: { label: 'QTN Sent', color: 'primary' },
  BOMReceived: { label: 'BOM Rcvd', color: 'secondary' },
  PISent: { label: 'PI Sent', color: 'warning' },
  Confirmed: { label: 'Confirmed', color: 'success' },
  Lost: { label: 'Lost', color: 'error' },
  Closed: { label: 'Closed', color: 'default' },
};

function StatusChip({ status }) {
  const config = STATUS_CONFIG[status] || { label: status, color: 'default' };
  return <Chip label={config.label} color={config.color} size="small" />;
}

export default function InquiriesPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [statusFilter, setStatusFilter] = useState('');
  const [customerFilter, setCustomerFilter] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useInquiries({
    search: debouncedSearch || undefined,
    status: statusFilter || undefined,
    customerId: customerFilter?.id || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const { data: stats } = useInquiryStats();
  const { data: customers } = useCustomerLookup();
  const deleteMutation = useDeleteInquiry();

  const statsData = stats?.data;
  const customerOptions = customers?.data ?? [];

  const columns = useMemo(
    () => [
      { field: 'inquiryNumber', headerName: 'Inquiry No' },
      {
        field: 'date',
        headerName: 'Date',
        renderCell: (row) => new Date(row.date).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' }),
      },
      { field: 'customerName', headerName: 'Customer' },
      { field: 'projectName', headerName: 'Project' },
      { field: 'source', headerName: 'Source' },
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
            <Tooltip title="Edit">
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/sales/inquiries/${row.id}`);
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
        toast.success('Inquiry deleted');
        setDeleteTarget(null);
      },
      onError: () => toast.error('Failed to delete inquiry'),
    });
  };

  const handleStatusCardClick = (status) => {
    setStatusFilter((prev) => (prev === status ? '' : status));
    setPage(0);
  };

  const rows = data?.data?.items ?? [];
  const totalCount = data?.data?.totalCount ?? 0;
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch && !statusFilter && !customerFilter;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Inquiries" />
        <EmptyState
          title="No inquiries yet"
          description="Create your first inquiry to start tracking sales leads."
          action={
            <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/sales/inquiries/new')}>
              New Inquiry
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Inquiries"
        action={
          <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/sales/inquiries/new')}>
            New Inquiry
          </Button>
        }
      />

      {/* Status Cards */}
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
                  minWidth: 100,
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

      {/* Filters */}
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search inquiries…"
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
          sx={{ minWidth: 140 }}
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
        onRowClick={(row) => navigate(`/sales/inquiries/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Inquiry"
        message={`Are you sure you want to delete "${deleteTarget?.inquiryNumber}"? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
