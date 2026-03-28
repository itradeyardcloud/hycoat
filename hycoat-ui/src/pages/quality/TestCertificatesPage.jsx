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
import { Add, Search, Edit, Delete, PictureAsPdf, Visibility } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { useTestCertificates, useDeleteTestCertificate } from '@/hooks/useTestCertificates';
import useDebounce from '@/hooks/useDebounce';

export default function TestCertificatesPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [dateFilter, setDateFilter] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useTestCertificates({
    search: debouncedSearch || undefined,
    date: dateFilter || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
    sortDesc: true,
  });
  const deleteMutation = useDeleteTestCertificate();

  const columns = useMemo(
    () => [
      { field: 'certificateNumber', headerName: 'Certificate #' },
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
      { field: 'customerName', headerName: 'Customer' },
      { field: 'workOrderNumber', headerName: 'Work Order' },
      { field: 'lotQuantity', headerName: 'Lot Qty' },
      { field: 'warranty', headerName: 'Warranty', renderCell: (row) => row.warranty ?? '—' },
      {
        field: 'overallStatus',
        headerName: 'Status',
        renderCell: (row) => (
          <Chip
            label={row.overallStatus}
            size="small"
            color={row.overallStatus === 'Pass' ? 'success' : 'error'}
          />
        ),
      },
      {
        field: 'hasPdf',
        headerName: 'PDF',
        renderCell: (row) =>
          row.hasPdf ? (
            <Chip icon={<PictureAsPdf fontSize="small" />} label="PDF" size="small" variant="outlined" color="primary" />
          ) : '—',
      },
      {
        field: 'actions',
        headerName: '',
        sortable: false,
        renderCell: (row) => (
          <Box sx={{ display: 'flex', gap: 0.5 }}>
            {row.hasPdf && (
              <Tooltip title="View PDF">
                <IconButton
                  size="small"
                  color="primary"
                  onClick={(e) => {
                    e.stopPropagation();
                    navigate(`/quality/test-certificates/${row.id}/preview`);
                  }}
                >
                  <Visibility fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
            <Tooltip title="Edit">
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/quality/test-certificates/${row.id}`);
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
        toast.success('Test certificate deleted');
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
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch && !dateFilter;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Test Certificates" />
        <EmptyState
          title="No test certificates yet"
          description="Test certificates are created from approved final inspections."
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Test Certificates"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/quality/test-certificates/new')}
          >
            New Certificate
          </Button>
        }
      />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <TextField
          placeholder="Search by Certificate# / Customer…"
          size="small"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(0);
          }}
          sx={{ minWidth: 280 }}
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
        onRowClick={(row) => navigate(`/quality/test-certificates/${row.id}`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Test Certificate"
        message="Are you sure you want to delete this test certificate? This action cannot be undone."
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
