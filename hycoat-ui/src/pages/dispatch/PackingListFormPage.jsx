import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm, Controller, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
  Box,
  Button,
  TextField,
  Grid,
  CircularProgress,
  Autocomplete,
  Typography,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
} from '@mui/material';
import { Add, Delete } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  usePackingList,
  useCreatePackingList,
  useUpdatePackingList,
} from '@/hooks/usePackingLists';
import { useProductionWorkOrderLookup } from '@/hooks/useProductionWorkOrders';
import { useWorkOrderLookup } from '@/hooks/useMaterialInwards';
import { useSectionProfileLookup } from '@/hooks/useSectionProfiles';

const lineSchema = z.object({
  sectionProfileId: z.number({ required_error: 'Required' }).min(1, 'Required'),
  quantity: z.coerce.number().min(1, 'Min 1'),
  lengthMM: z.coerce.number().min(1, 'Min 1'),
  bundleNumber: z.string().max(50).optional().or(z.literal('')),
  remarks: z.string().max(500).optional().or(z.literal('')),
});

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  productionWorkOrderId: z.number({ required_error: 'PWO is required' }).min(1, 'PWO is required'),
  workOrderId: z.number({ required_error: 'WO is required' }).min(1, 'Work Order is required'),
  remarks: z.string().max(1000).optional().or(z.literal('')),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});

const emptyLine = { sectionProfileId: 0, quantity: 0, lengthMM: 0, bundleNumber: '', remarks: '' };

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  productionWorkOrderId: 0,
  workOrderId: 0,
  remarks: '',
  lines: [{ ...emptyLine }],
};

export default function PackingListFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingExisting } = usePackingList(id);
  const { data: pwos } = useProductionWorkOrderLookup();
  const { data: wos } = useWorkOrderLookup();
  const { data: sectionProfiles } = useSectionProfileLookup();
  const createMutation = useCreatePackingList();
  const updateMutation = useUpdatePackingList();

  const pwoOptions = pwos?.data ?? [];
  const woOptions = wos?.data ?? [];
  const spOptions = sectionProfiles?.data ?? [];

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues,
  });

  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  useEffect(() => {
    if (existing?.data) {
      const d = existing.data;
      reset({
        date: d.date ? new Date(d.date).toISOString().split('T')[0] : '',
        productionWorkOrderId: d.productionWorkOrderId || 0,
        workOrderId: d.workOrderId || 0,
        remarks: d.remarks ?? '',
        lines: d.lines?.length
          ? d.lines.map((l) => ({
              sectionProfileId: l.sectionProfileId || 0,
              quantity: l.quantity ?? 0,
              lengthMM: l.lengthMM ?? 0,
              bundleNumber: l.bundleNumber ?? '',
              remarks: l.remarks ?? '',
            }))
          : [{ ...emptyLine }],
      });
    }
  }, [existing, reset]);

  const onSubmit = async (data) => {
    const payload = {
      ...data,
      remarks: data.remarks || null,
      lines: data.lines.map((l) => ({
        ...l,
        bundleNumber: l.bundleNumber || null,
        remarks: l.remarks || null,
      })),
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    try {
      await mutation.mutateAsync(mutationPayload);
      toast.success(`Packing list ${isEdit ? 'updated' : 'created'}`);
      navigate('/dispatch/packing-lists');
    } catch (err) {
      const msg =
        err.response?.data?.errors?.[0] ||
        err.response?.data?.message ||
        'Failed to save packing list';
      toast.error(msg);
    }
  };

  if (isEdit && loadingExisting) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <>
      <PageHeader title={isEdit ? 'Edit Packing List' : 'New Packing List'} />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <Grid container spacing={2} sx={{ maxWidth: 800, mb: 3 }}>
          {/* PWO */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="productionWorkOrderId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={pwoOptions}
                  getOptionLabel={(o) => o.name}
                  value={pwoOptions.find((p) => p.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? 0)}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Production Work Order"
                      required
                      error={!!errors.productionWorkOrderId}
                      helperText={errors.productionWorkOrderId?.message}
                    />
                  )}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          {/* Work Order */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="workOrderId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={woOptions}
                  getOptionLabel={(o) => o.name}
                  value={woOptions.find((w) => w.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? 0)}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Work Order"
                      required
                      error={!!errors.workOrderId}
                      helperText={errors.workOrderId?.message}
                    />
                  )}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          {/* Date */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="date"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  type="date"
                  label="Date"
                  required
                  fullWidth
                  size="small"
                  error={!!errors.date}
                  helperText={errors.date?.message}
                  slotProps={{ inputLabel: { shrink: true } }}
                />
              )}
            />
          </Grid>

          {/* Remarks */}
          <Grid size={12}>
            <Controller
              name="remarks"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Remarks"
                  fullWidth
                  size="small"
                  multiline
                  minRows={2}
                />
              )}
            />
          </Grid>
        </Grid>

        {/* Line Items */}
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
          <Typography variant="subtitle1" fontWeight={600}>
            Line Items
          </Typography>
          <Button size="small" startIcon={<Add />} onClick={() => append({ ...emptyLine })}>
            Add Line
          </Button>
        </Box>
        {errors.lines?.root && (
          <Typography color="error" variant="caption" sx={{ mb: 1, display: 'block' }}>
            {errors.lines.root.message}
          </Typography>
        )}

        <TableContainer component={Paper} variant="outlined" sx={{ mb: 3 }}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Section Profile</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 100 }}>Qty</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 120 }}>Length (mm)</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 120 }}>Bundle #</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Remarks</TableCell>
                <TableCell sx={{ width: 48 }} />
              </TableRow>
            </TableHead>
            <TableBody>
              {fields.map((field, index) => (
                <TableRow key={field.id}>
                  <TableCell>
                    <Controller
                      name={`lines.${index}.sectionProfileId`}
                      control={control}
                      render={({ field: f }) => (
                        <Autocomplete
                          size="small"
                          options={spOptions}
                          getOptionLabel={(o) => o.name}
                          value={spOptions.find((s) => s.id === f.value) ?? null}
                          onChange={(_, val) => f.onChange(val?.id ?? 0)}
                          renderInput={(params) => (
                            <TextField
                              {...params}
                              error={!!errors.lines?.[index]?.sectionProfileId}
                              placeholder="Select"
                              sx={{ minWidth: 160 }}
                            />
                          )}
                          isOptionEqualToValue={(opt, val) => opt.id === val.id}
                        />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lines.${index}.quantity`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField
                          {...f}
                          type="number"
                          size="small"
                          error={!!errors.lines?.[index]?.quantity}
                          sx={{ width: 80 }}
                        />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lines.${index}.lengthMM`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField
                          {...f}
                          type="number"
                          size="small"
                          error={!!errors.lines?.[index]?.lengthMM}
                          sx={{ width: 100 }}
                        />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lines.${index}.bundleNumber`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField {...f} size="small" sx={{ width: 100 }} />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lines.${index}.remarks`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField {...f} size="small" fullWidth />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    {fields.length > 1 && (
                      <IconButton size="small" color="error" onClick={() => remove(index)}>
                        <Delete fontSize="small" />
                      </IconButton>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        {/* Buttons */}
        <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
          <Button variant="outlined" onClick={() => navigate('/dispatch/packing-lists')}>
            Cancel
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isSubmitting || createMutation.isPending || updateMutation.isPending}
          >
            {isSubmitting || createMutation.isPending || updateMutation.isPending ? (
              <CircularProgress size={22} />
            ) : isEdit ? (
              'Update Packing List'
            ) : (
              'Save Packing List'
            )}
          </Button>
        </Box>
      </Box>
    </>
  );
}
