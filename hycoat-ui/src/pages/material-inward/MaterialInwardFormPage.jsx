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
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Stack,
  Card,
  CardContent,
  CardMedia,
  Tooltip,
} from '@mui/material';
import { Add, Delete, PhotoCamera, Close } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  useMaterialInward,
  useCreateMaterialInward,
  useUpdateMaterialInward,
  useUploadMaterialInwardPhotos,
  useDeleteMaterialInwardPhoto,
  useWorkOrderLookup,
} from '@/hooks/useMaterialInwards';
import { useCustomerLookup } from '@/hooks/useCustomers';
import { useSectionProfileLookup } from '@/hooks/useSectionProfiles';
import { useProcessTypes } from '@/hooks/useProcessTypes';
import { usePowderColorLookup } from '@/hooks/usePowderColors';

const lineSchema = z.object({
  id: z.number().nullable().optional(),
  sectionProfileId: z.number({ required_error: 'Section is required' }).min(1, 'Section is required'),
  lengthMM: z.coerce.number().min(0, 'Length must be ≥ 0'),
  qtyAsPerDC: z.coerce.number().int().min(1, 'DC Qty must be > 0'),
  qtyReceived: z.coerce.number().int().min(0, 'Received Qty must be ≥ 0'),
  weightKg: z.coerce.number().min(0).nullable().optional(),
  remarks: z.string().max(500).optional().or(z.literal('')),
});

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  customerId: z.number({ required_error: 'Customer is required' }).min(1, 'Customer is required'),
  workOrderId: z.number().nullable().optional(),
  customerDCNumber: z.string().max(100).optional().or(z.literal('')),
  customerDCDate: z.string().optional().or(z.literal('')),
  vehicleNumber: z.string().max(50).optional().or(z.literal('')),
  unloadingLocation: z.string().max(200).optional().or(z.literal('')),
  processTypeId: z.number().nullable().optional(),
  powderColorId: z.number().nullable().optional(),
  notes: z.string().max(2000).optional().or(z.literal('')),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});

const defaultLine = {
  id: null,
  sectionProfileId: 0,
  lengthMM: 0,
  qtyAsPerDC: 0,
  qtyReceived: 0,
  weightKg: null,
  remarks: '',
};

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  customerId: 0,
  workOrderId: null,
  customerDCNumber: '',
  customerDCDate: '',
  vehicleNumber: '',
  unloadingLocation: '',
  processTypeId: null,
  powderColorId: null,
  notes: '',
  lines: [{ ...defaultLine }],
};

const STATUS_COLORS = {
  Received: 'info',
  InspectionPending: 'warning',
  Inspected: 'primary',
  Stored: 'success',
};

export default function MaterialInwardFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;
  const fileInputRef = useRef(null);
  const [pendingPhotos, setPendingPhotos] = useState([]);

  const { data: existing, isLoading: loadingExisting } = useMaterialInward(id);
  const createMutation = useCreateMaterialInward();
  const updateMutation = useUpdateMaterialInward();
  const uploadPhotosMutation = useUploadMaterialInwardPhotos();
  const deletePhotoMutation = useDeleteMaterialInwardPhoto();

  const { data: workOrders } = useWorkOrderLookup();
  const { data: customers } = useCustomerLookup();
  const { data: sections } = useSectionProfileLookup();
  const { data: processTypes } = useProcessTypes();
  const { data: powderColors } = usePowderColorLookup();

  const woOptions = workOrders?.data ?? [];
  const customerOptions = customers?.data ?? [];
  const sectionOptions = sections?.data ?? [];
  const processTypeOptions = processTypes?.data ?? [];
  const powderColorOptions = powderColors?.data ?? [];

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

  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  const existingData = existing?.data;

  useEffect(() => {
    if (existingData) {
      reset({
        date: existingData.date ? existingData.date.split('T')[0] : '',
        customerId: existingData.customerId ?? 0,
        workOrderId: existingData.workOrderId ?? null,
        customerDCNumber: existingData.customerDCNumber ?? '',
        customerDCDate: existingData.customerDCDate
          ? existingData.customerDCDate.split('T')[0]
          : '',
        vehicleNumber: existingData.vehicleNumber ?? '',
        unloadingLocation: existingData.unloadingLocation ?? '',
        processTypeId: existingData.processTypeId ?? null,
        powderColorId: existingData.powderColorId ?? null,
        notes: existingData.notes ?? '',
        lines:
          existingData.lines?.map((l) => ({
            id: l.id,
            sectionProfileId: l.sectionProfileId,
            lengthMM: l.lengthMM,
            qtyAsPerDC: l.qtyAsPerDC,
            qtyReceived: l.qtyReceived,
            weightKg: l.weightKg ?? null,
            remarks: l.remarks ?? '',
          })) ?? [{ ...defaultLine }],
      });
    }
  }, [existingData, reset]);

  const handleWOChange = (wo) => {
    if (wo) {
      setValue('workOrderId', wo.id);
      setValue('customerId', wo.customerId);
      setValue('processTypeId', wo.processTypeId);
      setValue('powderColorId', wo.powderColorId ?? null);
    } else {
      setValue('workOrderId', null);
    }
  };

  const watchLines = watch('lines');

  const onSubmit = async (data) => {
    const payload = {
      ...data,
      workOrderId: data.workOrderId || null,
      processTypeId: data.processTypeId || null,
      powderColorId: data.powderColorId || null,
      customerDCDate: data.customerDCDate || null,
      weightKg: data.weightKg || null,
      lines: data.lines.map((l) => ({
        ...l,
        id: l.id || undefined,
        weightKg: l.weightKg || null,
        remarks: l.remarks || null,
      })),
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    try {
      const result = await mutation.mutateAsync(mutationPayload);
      const savedId = result?.data?.id || id;

      // Upload pending photos after create/update
      if (pendingPhotos.length > 0 && savedId) {
        await uploadPhotosMutation.mutateAsync({ id: savedId, files: pendingPhotos });
      }

      toast.success(`Material Inward ${isEdit ? 'updated' : 'created'}`);
      navigate('/material-inward/inwards');
    } catch (err) {
      const msg =
        err.response?.data?.errors?.[0] ||
        err.response?.data?.message ||
        'Failed to save material inward';
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

  const existingPhotos = existingData?.photos ?? [];

  return (
    <>
      <PageHeader
        title={isEdit ? `Edit ${existingData?.inwardNumber ?? 'Material Inward'}` : 'New Material Inward'}
        action={
          isEdit && existingData?.status ? (
            <Chip
              label={existingData.status}
              color={STATUS_COLORS[existingData.status] || 'default'}
            />
          ) : undefined
        }
      />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ maxWidth: 1000 }}>
        <Grid container spacing={2}>
          {/* Work Order */}
          <Grid size={{ xs: 12, sm: 6 }}>
            <Controller
              name="workOrderId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={woOptions}
                  getOptionLabel={(o) => `${o.woNumber} — ${o.customerName}`}
                  value={woOptions.find((w) => w.id === field.value) ?? null}
                  onChange={(_, val) => handleWOChange(val)}
                  renderInput={(params) => <TextField {...params} label="Work Order" />}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          {/* Customer */}
          <Grid size={{ xs: 12, sm: 6 }}>
            <Controller
              name="customerId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={customerOptions}
                  getOptionLabel={(o) => o.name}
                  value={customerOptions.find((c) => c.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? 0)}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Customer"
                      required
                      error={!!errors.customerId}
                      helperText={errors.customerId?.message}
                    />
                  )}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          {/* Date */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="date" label="Date" type="date" required errors={errors} shrink />
          </Grid>

          {/* Customer DC No */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="customerDCNumber" label="Customer DC No" errors={errors} />
          </Grid>

          {/* Customer DC Date */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="customerDCDate" label="Customer DC Date" type="date" errors={errors} shrink />
          </Grid>

          {/* Vehicle Number */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="vehicleNumber" label="Vehicle Number" errors={errors} />
          </Grid>

          {/* Unloading Location */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="unloadingLocation" label="Unloading Location" errors={errors} />
          </Grid>

          {/* Process Type */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="processTypeId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={processTypeOptions}
                  getOptionLabel={(o) => o.name}
                  value={processTypeOptions.find((pt) => pt.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? null)}
                  renderInput={(params) => <TextField {...params} label="Process Type" />}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          {/* Powder Color */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="powderColorId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={powderColorOptions}
                  getOptionLabel={(o) => o.name}
                  value={powderColorOptions.find((pc) => pc.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? null)}
                  renderInput={(params) => <TextField {...params} label="Powder Color" />}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          {/* Material Lines Section */}
          <Grid size={12}>
            <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 1, mt: 1 }} />
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
              <Typography variant="subtitle1" fontWeight={600}>
                Material Lines
              </Typography>
              <Button
                size="small"
                startIcon={<Add />}
                onClick={() => append({ ...defaultLine })}
              >
                Add Line
              </Button>
            </Box>

            {errors.lines?.root && (
              <Typography color="error" variant="body2" sx={{ mb: 1 }}>
                {errors.lines.root.message}
              </Typography>
            )}

            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>#</TableCell>
                    <TableCell sx={{ fontWeight: 600, minWidth: 180 }}>Section *</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Length (mm)</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>DC Qty *</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Received *</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Discrepancy</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Weight (kg)</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Remarks</TableCell>
                    <TableCell />
                  </TableRow>
                </TableHead>
                <TableBody>
                  {fields.map((field, index) => {
                    const disc =
                      (Number(watchLines?.[index]?.qtyReceived) || 0) -
                      (Number(watchLines?.[index]?.qtyAsPerDC) || 0);
                    const discColor = disc < 0 ? 'error.main' : disc > 0 ? 'warning.main' : 'success.main';

                    return (
                      <TableRow key={field.id}>
                        <TableCell>{index + 1}</TableCell>
                        <TableCell>
                          <Controller
                            name={`lines.${index}.sectionProfileId`}
                            control={control}
                            render={({ field: f }) => (
                              <Autocomplete
                                size="small"
                                options={sectionOptions}
                                getOptionLabel={(o) => o.name}
                                value={sectionOptions.find((s) => s.id === f.value) ?? null}
                                onChange={(_, val) => f.onChange(val?.id ?? 0)}
                                renderInput={(params) => (
                                  <TextField
                                    {...params}
                                    error={!!errors.lines?.[index]?.sectionProfileId}
                                    sx={{ minWidth: 150 }}
                                  />
                                )}
                                isOptionEqualToValue={(opt, val) => opt.id === val.id}
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
                                size="small"
                                type="number"
                                error={!!errors.lines?.[index]?.lengthMM}
                                sx={{ width: 100 }}
                              />
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <Controller
                            name={`lines.${index}.qtyAsPerDC`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField
                                {...f}
                                size="small"
                                type="number"
                                error={!!errors.lines?.[index]?.qtyAsPerDC}
                                sx={{ width: 80 }}
                              />
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <Controller
                            name={`lines.${index}.qtyReceived`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField
                                {...f}
                                size="small"
                                type="number"
                                error={!!errors.lines?.[index]?.qtyReceived}
                                sx={{ width: 80 }}
                              />
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <Typography
                            variant="body2"
                            fontWeight={600}
                            sx={{ color: discColor }}
                          >
                            {disc}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Controller
                            name={`lines.${index}.weightKg`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField
                                {...f}
                                value={f.value ?? ''}
                                onChange={(e) => f.onChange(e.target.value ? Number(e.target.value) : null)}
                                size="small"
                                type="number"
                                sx={{ width: 90 }}
                              />
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <Controller
                            name={`lines.${index}.remarks`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField {...f} size="small" sx={{ minWidth: 100 }} />
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
                    );
                  })}
                </TableBody>
              </Table>
            </TableContainer>
          </Grid>

          {/* Photos Section */}
          <Grid size={12}>
            <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 1, mt: 1 }} />
            <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1 }}>
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
              sx={{ mb: 1 }}
            >
              Take / Upload Photo
            </Button>

            <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap', gap: 1 }}>
              {/* Existing photos (edit mode) */}
              {existingPhotos.map((photo) => (
                <Card key={photo.id} sx={{ width: 120, position: 'relative' }}>
                  <CardMedia
                    component="img"
                    height="80"
                    image={photo.storedPath}
                    alt={photo.fileName}
                    sx={{ objectFit: 'cover' }}
                  />
                  <Tooltip title="Delete photo">
                    <IconButton
                      size="small"
                      sx={{ position: 'absolute', top: 0, right: 0, bgcolor: 'rgba(255,255,255,0.8)' }}
                      onClick={() => handleDeleteExistingPhoto(photo.id)}
                    >
                      <Close fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <CardContent sx={{ p: 0.5, '&:last-child': { pb: 0.5 } }}>
                    <Typography variant="caption" noWrap>
                      {photo.fileName}
                    </Typography>
                  </CardContent>
                </Card>
              ))}

              {/* Pending photos (not yet uploaded) */}
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
                      sx={{ position: 'absolute', top: 0, right: 0, bgcolor: 'rgba(255,255,255,0.8)' }}
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

          {/* Notes */}
          <Grid size={12}>
            <Field
              control={control}
              name="notes"
              label="Notes"
              multiline
              rows={3}
              errors={errors}
            />
          </Grid>

          {/* Buttons */}
          <Grid size={12}>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 1 }}>
              <Button variant="outlined" onClick={() => navigate('/material-inward/inwards')}>
                Cancel
              </Button>
              <Button
                type="submit"
                variant="contained"
                disabled={
                  isSubmitting || createMutation.isPending || updateMutation.isPending
                }
              >
                {isSubmitting || createMutation.isPending || updateMutation.isPending ? (
                  <CircularProgress size={22} />
                ) : isEdit ? (
                  'Update Material Inward'
                ) : (
                  'Save Material Inward'
                )}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Box>
    </>
  );
}

function Field({ control, name, label, errors, required, shrink, ...props }) {
  return (
    <Controller
      name={name}
      control={control}
      render={({ field }) => (
        <TextField
          {...field}
          {...props}
          label={label}
          required={required}
          fullWidth
          size="small"
          error={!!errors[name]}
          helperText={errors[name]?.message}
          slotProps={shrink ? { inputLabel: { shrink: true } } : undefined}
        />
      )}
    />
  );
}
