import { useState, useMemo } from 'react';
import {
  Button,
  IconButton,
  Box,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Grid,
} from '@mui/material';
import { Add, Edit, Delete } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import {
  useProcessTypes,
  useCreateProcessType,
  useUpdateProcessType,
  useDeleteProcessType,
} from '@/hooks/useProcessTypes';

export default function ProcessTypesPage() {
  const [formOpen, setFormOpen] = useState(false);
  const [editItem, setEditItem] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [formData, setFormData] = useState({ name: '', defaultRatePerSFT: '', description: '' });
  const [formErrors, setFormErrors] = useState({});

  const { data, isLoading } = useProcessTypes();
  const createMutation = useCreateProcessType();
  const updateMutation = useUpdateProcessType();
  const deleteMutation = useDeleteProcessType();

  const openCreate = () => {
    setEditItem(null);
    setFormData({ name: '', defaultRatePerSFT: '', description: '' });
    setFormErrors({});
    setFormOpen(true);
  };

  const openEdit = (row) => {
    setEditItem(row);
    setFormData({
      name: row.name ?? '',
      defaultRatePerSFT: row.defaultRatePerSFT ?? '',
      description: row.description ?? '',
    });
    setFormErrors({});
    setFormOpen(true);
  };

  const validate = () => {
    const errs = {};
    if (!formData.name.trim()) errs.name = 'Name is required';
    if (formData.defaultRatePerSFT && isNaN(Number(formData.defaultRatePerSFT)))
      errs.defaultRatePerSFT = 'Must be a number';
    setFormErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSave = () => {
    if (!validate()) return;

    const payload = {
      name: formData.name.trim(),
      defaultRatePerSFT: formData.defaultRatePerSFT ? Number(formData.defaultRatePerSFT) : null,
      description: formData.description || null,
    };

    const mutation = editItem ? updateMutation : createMutation;
    const mutationPayload = editItem ? { id: editItem.id, data: payload } : payload;

    mutation.mutate(mutationPayload, {
      onSuccess: () => {
        toast.success(`Process type ${editItem ? 'updated' : 'created'}`);
        setFormOpen(false);
      },
      onError: (err) => {
        const msg = err.response?.data?.errors?.[0] || err.response?.data?.message || 'Failed to save';
        toast.error(msg);
      },
    });
  };

  const handleDelete = () => {
    deleteMutation.mutate(deleteTarget.id, {
      onSuccess: () => {
        toast.success('Process type deleted');
        setDeleteTarget(null);
      },
      onError: () => toast.error('Failed to delete process type'),
    });
  };

  const columns = useMemo(
    () => [
      { field: 'name', headerName: 'Name' },
      { field: 'defaultRatePerSFT', headerName: 'Default Rate/SFT' },
      { field: 'description', headerName: 'Description' },
      {
        field: 'actions',
        headerName: '',
        sortable: false,
        renderCell: (row) => (
          <Box sx={{ display: 'flex', gap: 0.5 }}>
            <Tooltip title="Edit">
              <IconButton size="small" onClick={(e) => { e.stopPropagation(); openEdit(row); }}>
                <Edit fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Delete">
              <IconButton size="small" color="error" onClick={(e) => { e.stopPropagation(); setDeleteTarget(row); }}>
                <Delete fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>
        ),
      },
    ],
    [],
  );

  const listData = data?.data ?? data;
  const rows = Array.isArray(listData) ? listData : listData?.items ?? [];
  const isEmpty = !isLoading && rows.length === 0;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Process Types" />
        <EmptyState
          title="No process types yet"
          description="Add your first process type."
          action={
            <Button variant="contained" startIcon={<Add />} onClick={openCreate}>
              Add Process Type
            </Button>
          }
        />
        <FormDialog
          open={formOpen}
          onClose={() => setFormOpen(false)}
          onSave={handleSave}
          formData={formData}
          setFormData={setFormData}
          formErrors={formErrors}
          isEdit={!!editItem}
          isSaving={createMutation.isPending || updateMutation.isPending}
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title="Process Types"
        action={
          <Button variant="contained" startIcon={<Add />} onClick={openCreate}>
            Add Process Type
          </Button>
        }
      />

      <DataTable columns={columns} rows={rows} loading={isLoading} />

      <FormDialog
        open={formOpen}
        onClose={() => setFormOpen(false)}
        onSave={handleSave}
        formData={formData}
        setFormData={setFormData}
        formErrors={formErrors}
        isEdit={!!editItem}
        isSaving={createMutation.isPending || updateMutation.isPending}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Process Type"
        message={`Are you sure you want to delete "${deleteTarget?.name}"?`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}

function FormDialog({ open, onClose, onSave, formData, setFormData, formErrors, isEdit, isSaving }) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{isEdit ? 'Edit Process Type' : 'Add Process Type'}</DialogTitle>
      <DialogContent>
        <Grid container spacing={2} sx={{ mt: 0.5 }}>
          <Grid size={12}>
            <TextField
              label="Name"
              required
              fullWidth
              size="small"
              value={formData.name}
              onChange={(e) => setFormData((p) => ({ ...p, name: e.target.value }))}
              error={!!formErrors.name}
              helperText={formErrors.name}
            />
          </Grid>
          <Grid size={12}>
            <TextField
              label="Default Rate/SFT"
              fullWidth
              size="small"
              type="number"
              value={formData.defaultRatePerSFT}
              onChange={(e) => setFormData((p) => ({ ...p, defaultRatePerSFT: e.target.value }))}
              error={!!formErrors.defaultRatePerSFT}
              helperText={formErrors.defaultRatePerSFT}
            />
          </Grid>
          <Grid size={12}>
            <TextField
              label="Description"
              fullWidth
              size="small"
              multiline
              rows={2}
              value={formData.description}
              onChange={(e) => setFormData((p) => ({ ...p, description: e.target.value }))}
            />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" onClick={onSave} disabled={isSaving}>
          {isSaving ? 'Saving…' : 'Save'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
