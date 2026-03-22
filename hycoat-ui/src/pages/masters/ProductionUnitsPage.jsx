import { useState, useMemo } from 'react';
import {
  Button,
  IconButton,
  Box,
  Tooltip,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Grid,
  FormControlLabel,
  Switch,
} from '@mui/material';
import { Add, Edit, Delete } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import EmptyState from '@/components/common/EmptyState';
import {
  useProductionUnits,
  useCreateProductionUnit,
  useUpdateProductionUnit,
  useDeleteProductionUnit,
} from '@/hooks/useProductionUnits';

const emptyForm = {
  name: '',
  tankLengthMM: '',
  tankWidthMM: '',
  tankHeightMM: '',
  bucketLengthMM: '',
  bucketWidthMM: '',
  bucketHeightMM: '',
  conveyorLengthMtrs: '',
  isActive: true,
};

export default function ProductionUnitsPage() {
  const [formOpen, setFormOpen] = useState(false);
  const [editItem, setEditItem] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [formData, setFormData] = useState(emptyForm);
  const [formErrors, setFormErrors] = useState({});

  const { data, isLoading } = useProductionUnits();
  const createMutation = useCreateProductionUnit();
  const updateMutation = useUpdateProductionUnit();
  const deleteMutation = useDeleteProductionUnit();

  const openCreate = () => {
    setEditItem(null);
    setFormData(emptyForm);
    setFormErrors({});
    setFormOpen(true);
  };

  const openEdit = (row) => {
    setEditItem(row);
    setFormData({
      name: row.name ?? '',
      tankLengthMM: row.tankLengthMM ?? '',
      tankWidthMM: row.tankWidthMM ?? '',
      tankHeightMM: row.tankHeightMM ?? '',
      bucketLengthMM: row.bucketLengthMM ?? '',
      bucketWidthMM: row.bucketWidthMM ?? '',
      bucketHeightMM: row.bucketHeightMM ?? '',
      conveyorLengthMtrs: row.conveyorLengthMtrs ?? '',
      isActive: row.isActive ?? true,
    });
    setFormErrors({});
    setFormOpen(true);
  };

  const validate = () => {
    const errs = {};
    if (!formData.name.trim()) errs.name = 'Name is required';
    setFormErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSave = () => {
    if (!validate()) return;

    const numOrNull = (v) => (v !== '' && v != null ? Number(v) : null);
    const payload = {
      name: formData.name.trim(),
      tankLengthMM: numOrNull(formData.tankLengthMM),
      tankWidthMM: numOrNull(formData.tankWidthMM),
      tankHeightMM: numOrNull(formData.tankHeightMM),
      bucketLengthMM: numOrNull(formData.bucketLengthMM),
      bucketWidthMM: numOrNull(formData.bucketWidthMM),
      bucketHeightMM: numOrNull(formData.bucketHeightMM),
      conveyorLengthMtrs: numOrNull(formData.conveyorLengthMtrs),
      isActive: formData.isActive,
    };

    const mutation = editItem ? updateMutation : createMutation;
    const mutationPayload = editItem ? { id: editItem.id, data: payload } : payload;

    mutation.mutate(mutationPayload, {
      onSuccess: () => {
        toast.success(`Production unit ${editItem ? 'updated' : 'created'}`);
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
        toast.success('Production unit deleted');
        setDeleteTarget(null);
      },
      onError: () => toast.error('Failed to delete production unit'),
    });
  };

  const columns = useMemo(
    () => [
      { field: 'name', headerName: 'Name' },
      { field: 'tankLengthMM', headerName: 'Tank L (mm)' },
      { field: 'tankWidthMM', headerName: 'Tank W (mm)' },
      { field: 'tankHeightMM', headerName: 'Tank H (mm)' },
      { field: 'conveyorLengthMtrs', headerName: 'Conveyor (m)' },
      {
        field: 'isActive',
        headerName: 'Status',
        renderCell: (row) => (
          <Chip
            label={row.isActive ? 'Active' : 'Inactive'}
            color={row.isActive ? 'success' : 'default'}
            size="small"
            variant="outlined"
          />
        ),
      },
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

  const rows = Array.isArray(data) ? data : data?.items ?? [];
  const isEmpty = !isLoading && rows.length === 0;

  if (isEmpty) {
    return (
      <>
        <PageHeader title="Production Units" />
        <EmptyState
          title="No production units yet"
          description="Add your first production unit."
          action={
            <Button variant="contained" startIcon={<Add />} onClick={openCreate}>
              Add Production Unit
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
        title="Production Units"
        action={
          <Button variant="contained" startIcon={<Add />} onClick={openCreate}>
            Add Production Unit
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
        title="Delete Production Unit"
        message={`Are you sure you want to delete "${deleteTarget?.name}"?`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}

function FormDialog({ open, onClose, onSave, formData, setFormData, formErrors, isEdit, isSaving }) {
  const update = (field) => (e) => {
    const val = e.target.type === 'checkbox' ? e.target.checked : e.target.value;
    setFormData((p) => ({ ...p, [field]: val }));
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{isEdit ? 'Edit Production Unit' : 'Add Production Unit'}</DialogTitle>
      <DialogContent>
        <Grid container spacing={2} sx={{ mt: 0.5 }}>
          <Grid size={12}>
            <TextField
              label="Name"
              required
              fullWidth
              size="small"
              value={formData.name}
              onChange={update('name')}
              error={!!formErrors.name}
              helperText={formErrors.name}
            />
          </Grid>
          <Grid size={{ xs: 4 }}>
            <TextField label="Tank L (mm)" fullWidth size="small" type="number" value={formData.tankLengthMM} onChange={update('tankLengthMM')} />
          </Grid>
          <Grid size={{ xs: 4 }}>
            <TextField label="Tank W (mm)" fullWidth size="small" type="number" value={formData.tankWidthMM} onChange={update('tankWidthMM')} />
          </Grid>
          <Grid size={{ xs: 4 }}>
            <TextField label="Tank H (mm)" fullWidth size="small" type="number" value={formData.tankHeightMM} onChange={update('tankHeightMM')} />
          </Grid>
          <Grid size={{ xs: 4 }}>
            <TextField label="Bucket L (mm)" fullWidth size="small" type="number" value={formData.bucketLengthMM} onChange={update('bucketLengthMM')} />
          </Grid>
          <Grid size={{ xs: 4 }}>
            <TextField label="Bucket W (mm)" fullWidth size="small" type="number" value={formData.bucketWidthMM} onChange={update('bucketWidthMM')} />
          </Grid>
          <Grid size={{ xs: 4 }}>
            <TextField label="Bucket H (mm)" fullWidth size="small" type="number" value={formData.bucketHeightMM} onChange={update('bucketHeightMM')} />
          </Grid>
          <Grid size={{ xs: 6 }}>
            <TextField label="Conveyor (m)" fullWidth size="small" type="number" value={formData.conveyorLengthMtrs} onChange={update('conveyorLengthMtrs')} />
          </Grid>
          <Grid size={{ xs: 6 }}>
            <FormControlLabel
              control={<Switch checked={formData.isActive} onChange={update('isActive')} />}
              label="Active"
              sx={{ mt: 0.5 }}
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
