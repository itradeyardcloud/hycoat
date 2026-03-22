import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button, TextField, InputAdornment, IconButton, Box, Tooltip } from '@mui/material';
import { Add, Search, Edit, Delete, Image as ImageIcon } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import { useSectionProfiles, useDeleteSectionProfile } from '@/hooks/useSectionProfiles';
import useDebounce from '@/hooks/useDebounce';

export default function SectionProfilesPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const { data, isLoading } = useSectionProfiles({
    search: debouncedSearch || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
  });
  const deleteMutation = useDeleteSectionProfile();

  const columns = useMemo(
    () => [
      { field: 'sectionNumber', headerName: 'Section Number' },
      { field: 'type', headerName: 'Type' },
      { field: 'perimeterMM', headerName: 'Perimeter (mm)' },
      { field: 'weightPerMeter', headerName: 'Weight/m' },
      {
        field: 'drawingFileUrl',
        headerName: 'Drawing',
        sortable: false,
        renderCell: (row) =>
          row.drawingFileUrl ? (
            <Tooltip title="View drawing">
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  window.open(row.drawingFileUrl, '_blank', 'noopener,noreferrer');
                }}
              >
                <ImageIcon fontSize="small" color="primary" />
              </IconButton>
            </Tooltip>
          ) : (
            '—'
          ),
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
                  navigate(`/masters/section-profiles/${row.id}/edit`);
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
        toast.success('Section profile deleted');
        setDeleteTarget(null);
      },
      onError: () => toast.error('Failed to delete section profile'),
    });
  };

  const rows = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const isEmpty = !isLoading && rows.length === 0 && !debouncedSearch;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Section Profiles" />
        <EmptyState
          title="No section profiles yet"
          description="Add your first section profile to get started."
          action={
            <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/masters/section-profiles/new')}>
              Add Section Profile
            </Button>
          }
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Section Profiles"
        action={
          <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/masters/section-profiles/new')}>
            Add Section Profile
          </Button>
        }
      />

      <TextField
        placeholder="Search section profiles…"
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
        onRowClick={(row) => navigate(`/masters/section-profiles/${row.id}/edit`)}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Section Profile"
        message={`Are you sure you want to delete section "${deleteTarget?.sectionNumber}"?`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
