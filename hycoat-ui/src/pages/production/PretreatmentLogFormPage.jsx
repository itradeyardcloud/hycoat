import { useEffect, useMemo } from 'react';
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
  Accordion,
  AccordionSummary,
  AccordionDetails,
  ToggleButton,
  ToggleButtonGroup,
} from '@mui/material';
import { ExpandMore } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  usePretreatmentLog,
  useCreatePretreatmentLog,
  useUpdatePretreatmentLog,
} from '@/hooks/usePretreatmentLogs';
import { useProductionWorkOrderLookup } from '@/hooks/useProductionWorkOrders';

const TANK_SEQUENCE = [
  'Degreasing',
  'Water Rinse 1',
  'Etching',
  'Water Rinse 2',
  'Deoxidizing / De-smutting',
  'Water Rinse 3',
  'Chrome Conversion Coating',
  'Water Rinse 4',
  'DI Water Rinse',
  'Oven Dry',
];

const KEY_TANKS = ['Degreasing', 'Etching', 'Chrome Conversion Coating'];

const tankReadingSchema = z.object({
  tankName: z.string(),
  concentration: z.coerce.number().nullable().optional(),
  temperature: z.coerce.number().nullable().optional(),
  chemicalAdded: z.string().max(200).optional().or(z.literal('')),
  chemicalQty: z.coerce.number().nullable().optional(),
});

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  shift: z.enum(['Day', 'Night'], { required_error: 'Shift is required' }),
  productionWorkOrderId: z
    .number({ required_error: 'PWO is required' })
    .min(1, 'PWO is required'),
  basketNumber: z.coerce.number().int().min(1, 'Basket Number must be > 0'),
  startTime: z.string().optional().or(z.literal('')),
  endTime: z.string().optional().or(z.literal('')),
  etchTimeMins: z.coerce.number().positive('Must be > 0').nullable().optional(),
  remarks: z.string().max(1000).optional().or(z.literal('')),
  tankReadings: z.array(tankReadingSchema),
});

function getDefaultShift() {
  return new Date().getHours() < 18 ? 'Day' : 'Night';
}

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  shift: getDefaultShift(),
  productionWorkOrderId: 0,
  basketNumber: 1,
  startTime: '',
  endTime: '',
  etchTimeMins: null,
  remarks: '',
  tankReadings: TANK_SEQUENCE.map((name) => ({
    tankName: name,
    concentration: null,
    temperature: null,
    chemicalAdded: '',
    chemicalQty: null,
  })),
};

export default function PretreatmentLogFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingExisting } = usePretreatmentLog(id);
  const { data: pwos } = useProductionWorkOrderLookup({ status: 'InProgress' });
  const createMutation = useCreatePretreatmentLog();
  const updateMutation = useUpdatePretreatmentLog();

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

  const selectedPwoId = watch('productionWorkOrderId');

  // Populate form for edit
  useEffect(() => {
    if (existing?.data) {
      const d = existing.data;
      reset({
        date: d.date ? new Date(d.date).toISOString().split('T')[0] : '',
        shift: d.shift || 'Day',
        productionWorkOrderId: d.productionWorkOrderId || 0,
        basketNumber: d.basketNumber || 1,
        startTime: d.startTime ? d.startTime.substring(0, 5) : '',
        endTime: d.endTime ? d.endTime.substring(0, 5) : '',
        etchTimeMins: d.etchTimeMins ?? null,
        remarks: d.remarks ?? '',
        tankReadings: TANK_SEQUENCE.map((name) => {
          const existing = d.tankReadings?.find((t) => t.tankName === name);
          return {
            tankName: name,
            concentration: existing?.concentration ?? null,
            temperature: existing?.temperature ?? null,
            chemicalAdded: existing?.chemicalAdded ?? '',
            chemicalQty: existing?.chemicalQty ?? null,
          };
        }),
      });
    }
  }, [existing, reset]);

  const onSubmit = (data) => {
    // Convert time strings to TimeSpan format
    const payload = {
      ...data,
      startTime: data.startTime ? `${data.startTime}:00` : null,
      endTime: data.endTime ? `${data.endTime}:00` : null,
      etchTimeMins: data.etchTimeMins || null,
      tankReadings: data.tankReadings.map((t) => ({
        ...t,
        concentration: t.concentration || null,
        temperature: t.temperature || null,
        chemicalQty: t.chemicalQty || null,
      })),
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    mutation.mutate(mutationPayload, {
      onSuccess: () => {
        toast.success(`Pretreatment log ${isEdit ? 'updated' : 'created'}`);
        navigate('/production/pretreatment');
      },
      onError: (err) => {
        const msg =
          err.response?.data?.errors?.[0] ||
          err.response?.data?.message ||
          'Failed to save pretreatment log';
        toast.error(msg);
      },
    });
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
      <PageHeader title={isEdit ? 'Edit Pretreatment Log' : 'New Pretreatment Log'} />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ maxWidth: 800 }}>
        <Grid container spacing={2}>
          {/* PWO Autocomplete */}
          <Grid size={{ xs: 12 }}>
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

          {/* Shift */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="shift"
              control={control}
              render={({ field }) => (
                <Box>
                  <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>
                    Shift *
                  </Typography>
                  <ToggleButtonGroup
                    value={field.value}
                    exclusive
                    onChange={(_, val) => { if (val) field.onChange(val); }}
                    size="small"
                    fullWidth
                  >
                    <ToggleButton value="Day">Day</ToggleButton>
                    <ToggleButton value="Night">Night</ToggleButton>
                  </ToggleButtonGroup>
                  {errors.shift && (
                    <Typography variant="caption" color="error">
                      {errors.shift.message}
                    </Typography>
                  )}
                </Box>
              )}
            />
          </Grid>

          {/* Basket Number */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="basketNumber"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  type="number"
                  label="Basket Number"
                  required
                  fullWidth
                  size="small"
                  error={!!errors.basketNumber}
                  helperText={errors.basketNumber?.message}
                />
              )}
            />
          </Grid>

          {/* Start Time */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="startTime"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  type="time"
                  label="Start Time"
                  fullWidth
                  size="small"
                  slotProps={{ inputLabel: { shrink: true } }}
                />
              )}
            />
          </Grid>

          {/* End Time */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="endTime"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  type="time"
                  label="End Time"
                  fullWidth
                  size="small"
                  slotProps={{ inputLabel: { shrink: true } }}
                />
              )}
            />
          </Grid>

          {/* Etch Time */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="etchTimeMins"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  value={field.value ?? ''}
                  onChange={(e) => field.onChange(e.target.value === '' ? null : Number(e.target.value))}
                  type="number"
                  label="Etch Time (min)"
                  fullWidth
                  size="small"
                  error={!!errors.etchTimeMins}
                  helperText={errors.etchTimeMins?.message}
                />
              )}
            />
          </Grid>

          {/* Tank Readings */}
          <Grid size={12}>
            <Typography variant="subtitle1" sx={{ mt: 2, mb: 1, fontWeight: 600 }}>
              Tank Readings
            </Typography>
            {TANK_SEQUENCE.map((tankName, index) => {
              const isKey = KEY_TANKS.includes(tankName);
              return (
                <Accordion key={tankName} defaultExpanded={isKey} sx={{ mb: 0.5 }}>
                  <AccordionSummary expandIcon={<ExpandMore />}>
                    <Typography variant="body2" sx={{ fontWeight: isKey ? 600 : 400 }}>
                      {index + 1}. {tankName}
                    </Typography>
                  </AccordionSummary>
                  <AccordionDetails>
                    <Grid container spacing={1.5}>
                      <Grid size={{ xs: 6, sm: 3 }}>
                        <Controller
                          name={`tankReadings.${index}.concentration`}
                          control={control}
                          render={({ field }) => (
                            <TextField
                              {...field}
                              value={field.value ?? ''}
                              onChange={(e) =>
                                field.onChange(e.target.value === '' ? null : Number(e.target.value))
                              }
                              type="number"
                              label="Conc (point)"
                              fullWidth
                              size="small"
                            />
                          )}
                        />
                      </Grid>
                      <Grid size={{ xs: 6, sm: 3 }}>
                        <Controller
                          name={`tankReadings.${index}.temperature`}
                          control={control}
                          render={({ field }) => (
                            <TextField
                              {...field}
                              value={field.value ?? ''}
                              onChange={(e) =>
                                field.onChange(e.target.value === '' ? null : Number(e.target.value))
                              }
                              type="number"
                              label="Temp (°C)"
                              fullWidth
                              size="small"
                            />
                          )}
                        />
                      </Grid>
                      <Grid size={{ xs: 6, sm: 3 }}>
                        <Controller
                          name={`tankReadings.${index}.chemicalAdded`}
                          control={control}
                          render={({ field }) => (
                            <TextField
                              {...field}
                              label="Chemical Added"
                              fullWidth
                              size="small"
                            />
                          )}
                        />
                      </Grid>
                      <Grid size={{ xs: 6, sm: 3 }}>
                        <Controller
                          name={`tankReadings.${index}.chemicalQty`}
                          control={control}
                          render={({ field }) => (
                            <TextField
                              {...field}
                              value={field.value ?? ''}
                              onChange={(e) =>
                                field.onChange(e.target.value === '' ? null : Number(e.target.value))
                              }
                              type="number"
                              label="Qty"
                              fullWidth
                              size="small"
                            />
                          )}
                        />
                      </Grid>
                    </Grid>
                  </AccordionDetails>
                </Accordion>
              );
            })}
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
              <Button variant="outlined" onClick={() => navigate('/production/pretreatment')}>
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
                  'Save Log Entry'
                )}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Box>
    </>
  );
}
