import { useEffect, useRef, useState } from 'react';
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
  Stack,
  Card,
  CardMedia,
  CardContent,
  IconButton,
  Tooltip,
} from '@mui/material';
import { PhotoCamera, Close } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  useProductionLog,
  useCreateProductionLog,
  useUpdateProductionLog,
  useUploadProductionPhoto,
  useDeleteProductionPhoto,
} from '@/hooks/useProductionLogs';
import { useProductionWorkOrderLookup } from '@/hooks/useProductionWorkOrders';

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  shift: z.enum(['Day', 'Night'], { required_error: 'Shift is required' }),
  productionWorkOrderId: z
    .number({ required_error: 'PWO is required' })
    .min(1, 'PWO is required'),
  conveyorSpeedMtrPerMin: z.coerce
    .number()
    .min(0.1, 'Min 0.1')
    .max(5, 'Max 5')
    .nullable()
    .optional(),
  ovenTemperature: z.coerce
    .number()
    .min(150, 'Min 150')
    .max(300, 'Max 300')
    .nullable()
    .optional(),
  powderBatchNo: z.string().max(50).optional().or(z.literal('')),
  remarks: z.string().max(1000).optional().or(z.literal('')),
});

function getDefaultShift() {
  return new Date().getHours() < 18 ? 'Day' : 'Night';
}

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  shift: getDefaultShift(),
  productionWorkOrderId: 0,
  conveyorSpeedMtrPerMin: null,
  ovenTemperature: null,
  powderBatchNo: '',
  remarks: '',
};

export default function CoatingLogFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;
  const fileInputRef = useRef(null);
  const [pendingPhotos, setPendingPhotos] = useState([]);

  const { data: existing, isLoading: loadingExisting } = useProductionLog(id);
  const { data: pwos } = useProductionWorkOrderLookup({ status: 'InProgress' });
  const createMutation = useCreateProductionLog();
  const updateMutation = useUpdateProductionLog();
  const uploadPhotoMutation = useUploadProductionPhoto();
  const deletePhotoMutation = useDeleteProductionPhoto();

  const pwoOptions = pwos?.data ?? [];

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues,
  });

  // Populate form for edit
  useEffect(() => {
    if (existing?.data) {
      const d = existing.data;
      reset({
        date: d.date ? new Date(d.date).toISOString().split('T')[0] : '',
        shift: d.shift || 'Day',
        productionWorkOrderId: d.productionWorkOrderId || 0,
        conveyorSpeedMtrPerMin: d.conveyorSpeedMtrPerMin ?? null,
        ovenTemperature: d.ovenTemperature ?? null,
        powderBatchNo: d.powderBatchNo ?? '',
        remarks: d.remarks ?? '',
      });
    }
  }, [existing, reset]);

  const uploadPendingPhotos = async (logId) => {
    for (const file of pendingPhotos) {
      try {
        await uploadPhotoMutation.mutateAsync({ id: logId, file });
      } catch {
        toast.error(`Failed to upload ${file.name}`);
      }
    }
  };

  const onSubmit = async (data) => {
    const payload = {
      ...data,
      conveyorSpeedMtrPerMin: data.conveyorSpeedMtrPerMin || null,
      ovenTemperature: data.ovenTemperature || null,
      powderBatchNo: data.powderBatchNo || null,
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    try {
      const result = await mutation.mutateAsync(mutationPayload);
      const savedId = result?.data?.id || id;

      if (pendingPhotos.length > 0 && savedId) {
        await uploadPendingPhotos(savedId);
      }

      toast.success(`Coating log ${isEdit ? 'updated' : 'created'}`);
      navigate('/production/coating');
    } catch (err) {
      const msg =
        err.response?.data?.errors?.[0] ||
        err.response?.data?.message ||
        'Failed to save coating log';
      toast.error(msg);
    }
  };

  const handlePhotoSelect = (e) => {
    const files = Array.from(e.target.files || []);
    setPendingPhotos((prev) => [...prev, ...files]);
    e.target.value = '';
  };

  const handleDeleteExistingPhoto = (photoId) => {
    deletePhotoMutation.mutate(
      { id: Number(id), photoId },
      {
        onSuccess: () => toast.success('Photo deleted'),
        onError: () => toast.error('Failed to delete photo'),
      },
    );
  };

  if (isEdit && loadingExisting) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  const existingPhotos = existing?.data?.photos ?? [];

  return (
    <>
      <PageHeader title={isEdit ? 'Edit Coating Log' : 'New Coating Log'} />

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

          {/* Conveyor Speed */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="conveyorSpeedMtrPerMin"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  value={field.value ?? ''}
                  onChange={(e) => field.onChange(e.target.value === '' ? null : Number(e.target.value))}
                  type="number"
                  label="Conveyor Speed (m/min)"
                  fullWidth
                  size="small"
                  error={!!errors.conveyorSpeedMtrPerMin}
                  helperText={errors.conveyorSpeedMtrPerMin?.message}
                  slotProps={{ htmlInput: { step: 0.1, min: 0.1, max: 5 } }}
                />
              )}
            />
          </Grid>

          {/* Oven Temperature */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="ovenTemperature"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  value={field.value ?? ''}
                  onChange={(e) => field.onChange(e.target.value === '' ? null : Number(e.target.value))}
                  type="number"
                  label="Oven Temperature (°C)"
                  fullWidth
                  size="small"
                  error={!!errors.ovenTemperature}
                  helperText={errors.ovenTemperature?.message}
                  slotProps={{ htmlInput: { min: 150, max: 300 } }}
                />
              )}
            />
          </Grid>

          {/* Powder Batch No */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="powderBatchNo"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Powder Batch No"
                  fullWidth
                  size="small"
                  error={!!errors.powderBatchNo}
                  helperText={errors.powderBatchNo?.message}
                />
              )}
            />
          </Grid>

          {/* Photos Section */}
          <Grid size={12}>
            <Typography variant="subtitle1" sx={{ mt: 2, mb: 1, fontWeight: 600 }}>
              Photos
            </Typography>

            <input
              ref={fileInputRef}
              type="file"
              accept="image/*"
              capture="environment"
              multiple
              hidden
              onChange={handlePhotoSelect}
            />

            <Button
              variant="outlined"
              startIcon={<PhotoCamera />}
              onClick={() => fileInputRef.current?.click()}
              sx={{ mb: 1.5 }}
            >
              Take / Upload Photo
            </Button>

            <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap', gap: 1 }}>
              {/* Existing photos (edit mode) */}
              {existingPhotos.map((photo) => (
                <Card key={photo.id} sx={{ width: 130, position: 'relative' }}>
                  <CardMedia
                    component="img"
                    height="90"
                    image={photo.photoUrl}
                    alt={photo.description || 'Photo'}
                    sx={{ objectFit: 'cover' }}
                  />
                  <Tooltip title="Delete photo">
                    <IconButton
                      size="small"
                      sx={{
                        position: 'absolute',
                        top: 2,
                        right: 2,
                        bgcolor: 'rgba(255,255,255,0.85)',
                      }}
                      onClick={() => handleDeleteExistingPhoto(photo.id)}
                    >
                      <Close fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <CardContent sx={{ p: 0.5, '&:last-child': { pb: 0.5 } }}>
                    <Typography variant="caption" noWrap>
                      {photo.capturedAt
                        ? new Date(photo.capturedAt).toLocaleString('en-IN', {
                            day: '2-digit',
                            month: 'short',
                            hour: '2-digit',
                            minute: '2-digit',
                          })
                        : '—'}
                    </Typography>
                  </CardContent>
                </Card>
              ))}

              {/* Pending photos (not yet uploaded) */}
              {pendingPhotos.map((file, idx) => (
                <Card key={`pending-${idx}`} sx={{ width: 130, position: 'relative' }}>
                  <CardMedia
                    component="img"
                    height="90"
                    image={URL.createObjectURL(file)}
                    alt={file.name}
                    sx={{ objectFit: 'cover' }}
                  />
                  <Tooltip title="Remove">
                    <IconButton
                      size="small"
                      sx={{
                        position: 'absolute',
                        top: 2,
                        right: 2,
                        bgcolor: 'rgba(255,255,255,0.85)',
                      }}
                      onClick={() => setPendingPhotos((prev) => prev.filter((_, i) => i !== idx))}
                    >
                      <Close fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <CardContent sx={{ p: 0.5, '&:last-child': { pb: 0.5 } }}>
                    <Typography variant="caption" noWrap>
                      {file.name}
                    </Typography>
                  </CardContent>
                </Card>
              ))}
            </Stack>
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
              <Button variant="outlined" onClick={() => navigate('/production/coating')}>
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
                  'Save Coating Log'
                )}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Box>
    </>
  );
}
