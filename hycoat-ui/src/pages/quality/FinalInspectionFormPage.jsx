import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
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
  ToggleButton,
  ToggleButtonGroup,
  MenuItem,
} from '@mui/material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  useFinalInspection,
  useCreateFinalInspection,
  useUpdateFinalInspection,
} from '@/hooks/useFinalInspections';
import { useProductionWorkOrderLookup } from '@/hooks/useProductionWorkOrders';

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  productionWorkOrderId: z.number({ required_error: 'PWO is required' }).min(1, 'PWO is required'),
  lotQuantity: z.coerce.number().min(1, 'Min 1'),
  sampledQuantity: z.coerce.number().min(1, 'Min 1'),
  visualCheckStatus: z.enum(['Pass', 'Fail']).nullable().optional(),
  dftRecheckStatus: z.enum(['Pass', 'Fail']).nullable().optional(),
  shadeMatchFinalStatus: z.enum(['Pass', 'Fail']).nullable().optional(),
  overallStatus: z.enum(['Approved', 'Rejected', 'Rework'], { required_error: 'Required' }),
  remarks: z.string().max(1000).optional().or(z.literal('')),
});

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  productionWorkOrderId: 0,
  lotQuantity: 0,
  sampledQuantity: 0,
  visualCheckStatus: null,
  dftRecheckStatus: null,
  shadeMatchFinalStatus: null,
  overallStatus: 'Approved',
  remarks: '',
};

export default function FinalInspectionFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingExisting } = useFinalInspection(id);
  const { data: pwos } = useProductionWorkOrderLookup();
  const createMutation = useCreateFinalInspection();
  const updateMutation = useUpdateFinalInspection();

  const pwoOptions = pwos?.data ?? [];

  const {
    control,
    handleSubmit,
    reset,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues,
  });

  const visual = watch('visualCheckStatus');
  const dft = watch('dftRecheckStatus');
  const shade = watch('shadeMatchFinalStatus');

  // Auto-default overall status based on checks
  useEffect(() => {
    if (visual && dft && shade) {
      const allPass = visual === 'Pass' && dft === 'Pass' && shade === 'Pass';
      setValue('overallStatus', allPass ? 'Approved' : 'Rejected');
    }
  }, [visual, dft, shade, setValue]);

  useEffect(() => {
    if (existing?.data) {
      const d = existing.data;
      reset({
        date: d.date ? new Date(d.date).toISOString().split('T')[0] : '',
        productionWorkOrderId: d.productionWorkOrderId || 0,
        lotQuantity: d.lotQuantity ?? 0,
        sampledQuantity: d.sampledQuantity ?? 0,
        visualCheckStatus: d.visualCheckStatus ?? null,
        dftRecheckStatus: d.dftRecheckStatus ?? null,
        shadeMatchFinalStatus: d.shadeMatchFinalStatus ?? null,
        overallStatus: d.overallStatus ?? 'Approved',
        remarks: d.remarks ?? '',
      });
    }
  }, [existing, reset]);

  const onSubmit = async (data) => {
    const payload = {
      ...data,
      visualCheckStatus: data.visualCheckStatus || null,
      dftRecheckStatus: data.dftRecheckStatus || null,
      shadeMatchFinalStatus: data.shadeMatchFinalStatus || null,
      remarks: data.remarks || null,
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    try {
      await mutation.mutateAsync(mutationPayload);
      toast.success(`Final inspection ${isEdit ? 'updated' : 'created'}`);
      navigate('/quality/final');
    } catch (err) {
      const msg =
        err.response?.data?.errors?.[0] ||
        err.response?.data?.message ||
        'Failed to save final inspection';
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
      <PageHeader title={isEdit ? 'Edit Final Inspection' : 'New Final Inspection'} />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ maxWidth: 800 }}>
        <Grid container spacing={2}>
          {/* PWO */}
          <Grid size={{ xs: 12, sm: 8 }}>
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

          {/* Lot Quantity */}
          <Grid size={{ xs: 12, sm: 6 }}>
            <Controller
              name="lotQuantity"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  type="number"
                  label="Lot Quantity"
                  required
                  fullWidth
                  size="small"
                  error={!!errors.lotQuantity}
                  helperText={errors.lotQuantity?.message}
                />
              )}
            />
          </Grid>

          {/* Sampled Quantity */}
          <Grid size={{ xs: 12, sm: 6 }}>
            <Controller
              name="sampledQuantity"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  type="number"
                  label="Sampled Quantity"
                  required
                  fullWidth
                  size="small"
                  error={!!errors.sampledQuantity}
                  helperText={errors.sampledQuantity?.message}
                />
              )}
            />
          </Grid>

          {/* Visual Check */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="visualCheckStatus"
              control={control}
              render={({ field }) => (
                <Box>
                  <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>
                    Visual Check
                  </Typography>
                  <ToggleButtonGroup
                    value={field.value}
                    exclusive
                    onChange={(_, val) => field.onChange(val)}
                    size="small"
                    fullWidth
                  >
                    <ToggleButton value="Pass" color="success">Pass</ToggleButton>
                    <ToggleButton value="Fail" color="error">Fail</ToggleButton>
                  </ToggleButtonGroup>
                </Box>
              )}
            />
          </Grid>

          {/* DFT Re-check */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="dftRecheckStatus"
              control={control}
              render={({ field }) => (
                <Box>
                  <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>
                    DFT Re-check
                  </Typography>
                  <ToggleButtonGroup
                    value={field.value}
                    exclusive
                    onChange={(_, val) => field.onChange(val)}
                    size="small"
                    fullWidth
                  >
                    <ToggleButton value="Pass" color="success">Pass</ToggleButton>
                    <ToggleButton value="Fail" color="error">Fail</ToggleButton>
                  </ToggleButtonGroup>
                </Box>
              )}
            />
          </Grid>

          {/* Shade Match Final */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="shadeMatchFinalStatus"
              control={control}
              render={({ field }) => (
                <Box>
                  <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>
                    Shade Match Final
                  </Typography>
                  <ToggleButtonGroup
                    value={field.value}
                    exclusive
                    onChange={(_, val) => field.onChange(val)}
                    size="small"
                    fullWidth
                  >
                    <ToggleButton value="Pass" color="success">Pass</ToggleButton>
                    <ToggleButton value="Fail" color="error">Fail</ToggleButton>
                  </ToggleButtonGroup>
                </Box>
              )}
            />
          </Grid>

          {/* Overall Status */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="overallStatus"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  select
                  label="Overall Status"
                  required
                  fullWidth
                  size="small"
                  error={!!errors.overallStatus}
                  helperText={errors.overallStatus?.message}
                >
                  <MenuItem value="Approved">Approved</MenuItem>
                  <MenuItem value="Rejected">Rejected</MenuItem>
                  <MenuItem value="Rework">Rework</MenuItem>
                </TextField>
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
                  error={!!errors.remarks}
                  helperText={errors.remarks?.message}
                />
              )}
            />
          </Grid>

          {/* Buttons */}
          <Grid size={12}>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 1 }}>
              <Button variant="outlined" onClick={() => navigate('/quality/final')}>
                Cancel
              </Button>
              <Button
                type="submit"
                variant="contained"
                disabled={isSubmitting || createMutation.isPending || updateMutation.isPending}
              >
                {isSubmitting || createMutation.isPending || updateMutation.isPending ? (
                  <CircularProgress size={22} />
                ) : (
                  'Save Inspection'
                )}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Box>
    </>
  );
}
