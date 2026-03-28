import { useEffect, useRef, useState } from 'react';
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
  Chip,
  Typography,
  ToggleButton,
  ToggleButtonGroup,
  Switch,
  FormControlLabel,
  MenuItem,
  Card,
  CardContent,
  CardMedia,
  Stack,
  IconButton,
  Tooltip,
  Divider,
} from '@mui/material';
import { PhotoCamera, Close, CheckCircle, Cancel } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  useIncomingInspection,
  useCreateIncomingInspection,
  useUpdateIncomingInspection,
  useUploadInspectionPhotos,
} from '@/hooks/useIncomingInspections';
import { useMaterialInward, useMaterialInwardLookup } from '@/hooks/useMaterialInwards';

const lineSchema = z.object({
  materialInwardLineId: z.number(),
  watermarkOk: z.boolean().nullable().optional(),
  scratchOk: z.boolean().nullable().optional(),
  dentOk: z.boolean().nullable().optional(),
  dimensionalCheckOk: z.boolean().nullable().optional(),
  buffingRequired: z.boolean(),
  buffingCharge: z.coerce.number().min(0).nullable().optional(),
  status: z.enum(['Pass', 'Fail', 'Conditional'], { required_error: 'Status is required' }),
  remarks: z.string().max(500).optional().or(z.literal('')),
});

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  materialInwardId: z.number({ required_error: 'Material Inward is required' }).min(1, 'Material Inward is required'),
  remarks: z.string().max(2000).optional().or(z.literal('')),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  materialInwardId: 0,
  remarks: '',
  lines: [],
};

const STATUS_COLORS = {
  Pass: 'success',
  Fail: 'error',
  Conditional: 'warning',
};

export default function IncomingInspectionFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;
  const fileInputRef = useRef(null);
  const [pendingPhotos, setPendingPhotos] = useState([]);
  const [selectedInwardId, setSelectedInwardId] = useState(null);

  const { data: existing, isLoading: loadingExisting } = useIncomingInspection(id);
  const createMutation = useCreateIncomingInspection();
  const updateMutation = useUpdateIncomingInspection();
  const uploadPhotosMutation = useUploadInspectionPhotos();

  // For new inspections, only show inwards without inspection
  const { data: inwardLookup } = useMaterialInwardLookup({ hasInspection: false });
  // When editing, also fetch the already-linked inward
  const { data: selectedInward } = useMaterialInward(
    isEdit ? existing?.data?.materialInwardId : selectedInwardId,
  );

  const inwardOptions = inwardLookup?.data ?? [];

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

  const { fields, replace } = useFieldArray({ control, name: 'lines' });

  const existingData = existing?.data;

  // Populate form on edit
  useEffect(() => {
    if (existingData) {
      reset({
        date: existingData.date ? existingData.date.split('T')[0] : '',
        materialInwardId: existingData.materialInwardId ?? 0,
        remarks: existingData.remarks ?? '',
        lines:
          existingData.lines?.map((l) => ({
            materialInwardLineId: l.materialInwardLineId,
            watermarkOk: l.watermarkOk ?? null,
            scratchOk: l.scratchOk ?? null,
            dentOk: l.dentOk ?? null,
            dimensionalCheckOk: l.dimensionalCheckOk ?? null,
            buffingRequired: l.buffingRequired ?? false,
            buffingCharge: l.buffingCharge ?? null,
            status: l.status ?? 'Pass',
            remarks: l.remarks ?? '',
          })) ?? [],
      });
    }
  }, [existingData, reset]);

  // Auto-populate lines when selecting a Material Inward (new mode)
  const inwardDetail = selectedInward?.data;
  useEffect(() => {
    if (!isEdit && inwardDetail?.lines) {
      const newLines = inwardDetail.lines.map((l) => ({
        materialInwardLineId: l.id,
        watermarkOk: null,
        scratchOk: null,
        dentOk: null,
        dimensionalCheckOk: null,
        buffingRequired: false,
        buffingCharge: null,
        status: 'Pass',
        remarks: '',
      }));
      replace(newLines);
    }
  }, [inwardDetail, isEdit, replace]);

  const watchLines = watch('lines');

  // Derive overall status from lines
  const overallStatus = (() => {
    if (!watchLines || watchLines.length === 0) return null;
    const statuses = watchLines.map((l) => l.status);
    if (statuses.includes('Fail')) return 'Fail';
    if (statuses.includes('Conditional')) return 'Conditional';
    return 'Pass';
  })();

  const handleInwardChange = (inward) => {
    if (inward) {
      setValue('materialInwardId', inward.id);
      setSelectedInwardId(inward.id);
    } else {
      setValue('materialInwardId', 0);
      setSelectedInwardId(null);
      replace([]);
    }
  };

  const onSubmit = async (data) => {
    const payload = {
      ...data,
      lines: data.lines.map((l) => ({
        ...l,
        buffingCharge: l.buffingRequired ? (l.buffingCharge ?? 0) : null,
        remarks: l.remarks || null,
      })),
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    try {
      const result = await mutation.mutateAsync(mutationPayload);
      const savedId = result?.data?.id || id;

      if (pendingPhotos.length > 0 && savedId) {
        await uploadPhotosMutation.mutateAsync({ id: savedId, files: pendingPhotos });
      }

      toast.success(`Inspection ${isEdit ? 'updated' : 'created'}`);
      navigate('/material-inward/inspections');
    } catch (err) {
      const msg =
        err.response?.data?.errors?.[0] ||
        err.response?.data?.message ||
        'Failed to save inspection';
      toast.error(msg);
    }
  };

  const handlePhotoSelect = (e) => {
    const files = Array.from(e.target.files || []);
    setPendingPhotos((prev) => [...prev, ...files]);
    e.target.value = '';
  };

  if (isEdit && loadingExisting) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  // Get section info for display
  const inwardLines = inwardDetail?.lines ?? existingData?.lines ?? [];
  const existingPhotos = existingData?.photos ?? [];

  return (
    <>
      <PageHeader
        title={
          isEdit
            ? `Edit ${existingData?.inspectionNumber ?? 'Inspection'}`
            : 'New Incoming Inspection'
        }
        action={
          overallStatus ? (
            <Chip
              label={`Overall: ${overallStatus}`}
              color={STATUS_COLORS[overallStatus] || 'default'}
            />
          ) : undefined
        }
      />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ maxWidth: 900 }}>
        <Grid container spacing={2}>
          {/* Material Inward */}
          <Grid size={{ xs: 12, sm: 6 }}>
            {isEdit ? (
              <TextField
                label="Material Inward"
                value={existingData?.inwardNumber ?? ''}
                fullWidth
                size="small"
                disabled
              />
            ) : (
              <Controller
                name="materialInwardId"
                control={control}
                render={({ field }) => (
                  <Autocomplete
                    size="small"
                    options={inwardOptions}
                    getOptionLabel={(o) => o.name}
                    value={inwardOptions.find((i) => i.id === field.value) ?? null}
                    onChange={(_, val) => handleInwardChange(val)}
                    renderInput={(params) => (
                      <TextField
                        {...params}
                        label="Material Inward"
                        required
                        error={!!errors.materialInwardId}
                        helperText={errors.materialInwardId?.message}
                      />
                    )}
                    isOptionEqualToValue={(opt, val) => opt.id === val.id}
                  />
                )}
              />
            )}
          </Grid>

          {/* Date */}
          <Grid size={{ xs: 12, sm: 3 }}>
            <Controller
              name="date"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Date"
                  type="date"
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

          {/* Read-only info */}
          {inwardDetail && (
            <>
              <Grid size={{ xs: 12, sm: 3 }}>
                <TextField
                  label="Customer"
                  value={inwardDetail.customerName ?? ''}
                  fullWidth
                  size="small"
                  disabled
                />
              </Grid>
            </>
          )}

          {/* Inspection Lines */}
          <Grid size={12}>
            <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 1, mt: 1 }} />
            <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1 }}>
              Inspection Lines
            </Typography>

            {fields.length === 0 && (
              <Typography color="text.secondary" variant="body2" sx={{ mb: 2 }}>
                Select a Material Inward to populate inspection lines.
              </Typography>
            )}

            {errors.lines?.root && (
              <Typography color="error" variant="body2" sx={{ mb: 1 }}>
                {errors.lines.root.message}
              </Typography>
            )}

            <Stack spacing={2}>
              {fields.map((field, index) => {
                // Find the matching inward line for display info
                const inwardLine = inwardLines.find(
                  (l) => l.id === field.materialInwardLineId || l.materialInwardLineId === field.materialInwardLineId,
                );
                const sectionLabel = inwardLine?.sectionNumber ?? `Line ${index + 1}`;
                const qtyLabel = inwardLine?.qtyReceived != null ? `${inwardLine.qtyReceived} pcs` : '';

                return (
                  <Card key={field.id} variant="outlined">
                    <CardContent>
                      <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 1.5 }}>
                        {sectionLabel} {qtyLabel && `(${qtyLabel})`}
                      </Typography>

                      <Grid container spacing={2}>
                        {/* Check toggles */}
                        {[
                          { name: 'watermarkOk', label: 'Watermark' },
                          { name: 'scratchOk', label: 'Scratch' },
                          { name: 'dentOk', label: 'Dent' },
                          { name: 'dimensionalCheckOk', label: 'Dimensional' },
                        ].map((check) => (
                          <Grid key={check.name} size={{ xs: 6, sm: 3 }}>
                            <Typography variant="caption" color="text.secondary">
                              {check.label}
                            </Typography>
                            <Controller
                              name={`lines.${index}.${check.name}`}
                              control={control}
                              render={({ field: f }) => (
                                <ToggleButtonGroup
                                  size="small"
                                  exclusive
                                  value={f.value === true ? 'ok' : f.value === false ? 'defect' : null}
                                  onChange={(_, val) => {
                                    if (val === 'ok') f.onChange(true);
                                    else if (val === 'defect') f.onChange(false);
                                    else f.onChange(null);
                                  }}
                                  sx={{ display: 'flex' }}
                                >
                                  <ToggleButton value="ok" color="success" sx={{ flex: 1 }}>
                                    <CheckCircle fontSize="small" sx={{ mr: 0.5 }} /> OK
                                  </ToggleButton>
                                  <ToggleButton value="defect" color="error" sx={{ flex: 1 }}>
                                    <Cancel fontSize="small" sx={{ mr: 0.5 }} /> Defect
                                  </ToggleButton>
                                </ToggleButtonGroup>
                              )}
                            />
                          </Grid>
                        ))}

                        <Grid size={12}>
                          <Divider sx={{ my: 0.5 }} />
                        </Grid>

                        {/* Buffing */}
                        <Grid size={{ xs: 6, sm: 3 }}>
                          <Controller
                            name={`lines.${index}.buffingRequired`}
                            control={control}
                            render={({ field: f }) => (
                              <FormControlLabel
                                control={
                                  <Switch
                                    checked={f.value}
                                    onChange={(e) => f.onChange(e.target.checked)}
                                    size="small"
                                  />
                                }
                                label="Buffing Required"
                              />
                            )}
                          />
                        </Grid>

                        {watchLines?.[index]?.buffingRequired && (
                          <Grid size={{ xs: 6, sm: 3 }}>
                            <Controller
                              name={`lines.${index}.buffingCharge`}
                              control={control}
                              render={({ field: f }) => (
                                <TextField
                                  {...f}
                                  value={f.value ?? ''}
                                  onChange={(e) =>
                                    f.onChange(e.target.value ? Number(e.target.value) : null)
                                  }
                                  label="Buffing Charge (₹)"
                                  type="number"
                                  size="small"
                                  fullWidth
                                />
                              )}
                            />
                          </Grid>
                        )}

                        {/* Line Status */}
                        <Grid size={{ xs: 6, sm: 3 }}>
                          <Controller
                            name={`lines.${index}.status`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField
                                {...f}
                                select
                                label="Status"
                                size="small"
                                fullWidth
                                required
                                error={!!errors.lines?.[index]?.status}
                              >
                                <MenuItem value="Pass">Pass</MenuItem>
                                <MenuItem value="Fail">Fail</MenuItem>
                                <MenuItem value="Conditional">Conditional</MenuItem>
                              </TextField>
                            )}
                          />
                        </Grid>

                        {/* Line Remarks */}
                        <Grid size={{ xs: 12, sm: 6 }}>
                          <Controller
                            name={`lines.${index}.remarks`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField {...f} label="Remarks" size="small" fullWidth />
                            )}
                          />
                        </Grid>
                      </Grid>
                    </CardContent>
                  </Card>
                );
              })}
            </Stack>
          </Grid>

          {/* Defect Photos */}
          <Grid size={12}>
            <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 1, mt: 1 }} />
            <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1 }}>
              Defect Photos
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
              sx={{ mb: 1 }}
            >
              Upload Photo
            </Button>

            <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap', gap: 1 }}>
              {existingPhotos.map((photo) => (
                <Card key={photo.id} sx={{ width: 120, position: 'relative' }}>
                  <CardMedia
                    component="img"
                    height="80"
                    image={photo.storedPath}
                    alt={photo.fileName}
                    sx={{ objectFit: 'cover' }}
                  />
                  <CardContent sx={{ p: 0.5, '&:last-child': { pb: 0.5 } }}>
                    <Typography variant="caption" noWrap>
                      {photo.fileName}
                    </Typography>
                  </CardContent>
                </Card>
              ))}

              {pendingPhotos.map((file, idx) => (
                <Card key={`pending-${idx}`} sx={{ width: 120, position: 'relative' }}>
                  <CardMedia
                    component="img"
                    height="80"
                    image={URL.createObjectURL(file)}
                    alt={file.name}
                    sx={{ objectFit: 'cover' }}
                  />
                  <Tooltip title="Remove">
                    <IconButton
                      size="small"
                      sx={{
                        position: 'absolute',
                        top: 0,
                        right: 0,
                        bgcolor: 'rgba(255,255,255,0.8)',
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

          {/* Overall Remarks */}
          <Grid size={12}>
            <Controller
              name="remarks"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Remarks"
                  multiline
                  rows={3}
                  fullWidth
                  size="small"
                />
              )}
            />
          </Grid>

          {/* Buttons */}
          <Grid size={12}>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 1 }}>
              <Button
                variant="outlined"
                onClick={() => navigate('/material-inward/inspections')}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                variant="contained"
                disabled={
                  isSubmitting ||
                  createMutation.isPending ||
                  updateMutation.isPending ||
                  fields.length === 0
                }
              >
                {isSubmitting || createMutation.isPending || updateMutation.isPending ? (
                  <CircularProgress size={22} />
                ) : isEdit ? (
                  'Update Inspection'
                ) : (
                  'Save Inspection Report'
                )}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Box>
    </>
  );
}
