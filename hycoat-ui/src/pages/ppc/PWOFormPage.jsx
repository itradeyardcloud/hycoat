import { useEffect, useMemo } from 'react';
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
  Card,
  CardContent,
  RadioGroup,
  Radio,
  FormControlLabel,
  FormLabel,
  FormControl,
  Alert,
} from '@mui/material';
import { Add, Delete, Calculate } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  useProductionWorkOrder,
  useCreateProductionWorkOrder,
  useUpdateProductionWorkOrder,
  useCalculateProductionTime,
} from '@/hooks/useProductionWorkOrders';
import { useWorkOrderLookup } from '@/hooks/useMaterialInwards';
import { useSectionProfileLookup } from '@/hooks/useSectionProfiles';
import { useProcessTypes } from '@/hooks/useProcessTypes';
import { usePowderColorLookup } from '@/hooks/usePowderColors';
import { useProductionUnits } from '@/hooks/useProductionUnits';

const lineSchema = z.object({
  sectionProfileId: z
    .number({ required_error: 'Section is required' })
    .min(1, 'Section is required'),
  customerDCNo: z.string().max(100).optional().or(z.literal('')),
  quantity: z.coerce.number().int().min(1, 'Qty must be > 0'),
  lengthMM: z.coerce.number().min(0, 'Length must be ≥ 0'),
  specialInstructions: z.string().max(500).optional().or(z.literal('')),
});

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  workOrderId: z.number({ required_error: 'Work Order is required' }).min(1, 'Work Order is required'),
  processTypeId: z.number({ required_error: 'Process Type is required' }).min(1, 'Process Type is required'),
  powderColorId: z.number().nullable().optional(),
  productionUnitId: z.number({ required_error: 'Unit is required' }).min(1, 'Unit is required'),
  shiftAllocation: z.string().min(1, 'Shift is required'),
  startDate: z.string().optional().or(z.literal('')),
  dispatchDate: z.string().optional().or(z.literal('')),
  packingType: z.string().max(100).optional().or(z.literal('')),
  specialInstructions: z.string().max(2000).optional().or(z.literal('')),
  lineItems: z.array(lineSchema).min(1, 'At least one line item is required'),
});

const defaultLine = {
  sectionProfileId: 0,
  customerDCNo: '',
  quantity: 0,
  lengthMM: 0,
  specialInstructions: '',
};

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  workOrderId: 0,
  processTypeId: 0,
  powderColorId: null,
  productionUnitId: 0,
  shiftAllocation: 'Day',
  startDate: '',
  dispatchDate: '',
  packingType: '',
  specialInstructions: '',
  lineItems: [{ ...defaultLine }],
};

const STATUS_COLORS = {
  Created: 'default',
  Scheduled: 'info',
  InProgress: 'warning',
  Complete: 'success',
};

export default function PWOFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingExisting } = useProductionWorkOrder(id);
  const createMutation = useCreateProductionWorkOrder();
  const updateMutation = useUpdateProductionWorkOrder();
  const calcMutation = useCalculateProductionTime();

  const { data: workOrders } = useWorkOrderLookup();
  const { data: sections } = useSectionProfileLookup();
  const { data: processTypes } = useProcessTypes();
  const { data: powderColors } = usePowderColorLookup();
  const { data: productionUnits } = useProductionUnits();

  const woOptions = workOrders?.data ?? [];
  const sectionOptions = sections?.data ?? [];
  const processTypeOptions = processTypes?.data?.items ?? processTypes?.data ?? [];
  const powderColorOptions = powderColors?.data ?? [];
  const unitOptions = productionUnits?.data?.items ?? productionUnits?.data ?? [];

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

  const { fields, append, remove } = useFieldArray({ control, name: 'lineItems' });

  const existingData = existing?.data;

  useEffect(() => {
    if (existingData) {
      reset({
        date: existingData.date ? existingData.date.split('T')[0] : '',
        workOrderId: existingData.workOrderId ?? 0,
        processTypeId: existingData.processTypeId ?? 0,
        powderColorId: existingData.powderColorId ?? null,
        productionUnitId: existingData.productionUnitId ?? 0,
        shiftAllocation: existingData.shiftAllocation ?? 'Day',
        startDate: existingData.startDate ? existingData.startDate.split('T')[0] : '',
        dispatchDate: existingData.dispatchDate ? existingData.dispatchDate.split('T')[0] : '',
        packingType: existingData.packingType ?? '',
        specialInstructions: existingData.specialInstructions ?? '',
        lineItems:
          existingData.lineItems?.map((l) => ({
            sectionProfileId: l.sectionProfileId,
            customerDCNo: l.customerDCNo ?? '',
            quantity: l.quantity,
            lengthMM: l.lengthMM,
            specialInstructions: l.specialInstructions ?? '',
          })) ?? [{ ...defaultLine }],
      });
    }
  }, [existingData, reset]);

  const handleWOChange = (wo) => {
    if (wo) {
      setValue('workOrderId', wo.id);
      setValue('processTypeId', wo.processTypeId);
      if (wo.powderColorId) setValue('powderColorId', wo.powderColorId);
    } else {
      setValue('workOrderId', 0);
    }
  };

  const watchLines = watch('lineItems');
  const watchUnitId = watch('productionUnitId');

  const timeCalcResult = calcMutation.data?.data;

  const handleCalcTime = () => {
    const validLines = (watchLines || []).filter(
      (l) => l.sectionProfileId > 0 && l.quantity > 0 && l.lengthMM > 0,
    );
    if (!watchUnitId || validLines.length === 0) {
      toast.error('Select a production unit and add at least one valid line item');
      return;
    }
    calcMutation.mutate({
      productionUnitId: watchUnitId,
      lineItems: validLines.map((l) => ({
        sectionProfileId: l.sectionProfileId,
        quantity: l.quantity,
        lengthMM: l.lengthMM,
      })),
    });
  };

  const lineCalcMap = useMemo(() => {
    const map = {};
    if (timeCalcResult?.lines) {
      timeCalcResult.lines.forEach((lc) => {
        map[lc.sectionProfileId] = lc.calc;
      });
    }
    return map;
  }, [timeCalcResult]);

  const onSubmit = async (data) => {
    const payload = {
      ...data,
      powderColorId: data.powderColorId || null,
      startDate: data.startDate || null,
      dispatchDate: data.dispatchDate || null,
      packingType: data.packingType || null,
      specialInstructions: data.specialInstructions || null,
      lineItems: data.lineItems.map((l) => ({
        ...l,
        customerDCNo: l.customerDCNo || null,
        specialInstructions: l.specialInstructions || null,
      })),
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    try {
      await mutation.mutateAsync(mutationPayload);
      toast.success(`Production Work Order ${isEdit ? 'updated' : 'created'}`);
      navigate('/ppc/work-orders');
    } catch (err) {
      const msg =
        err.response?.data?.errors?.[0] ||
        err.response?.data?.message ||
        'Failed to save PWO';
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

  const isReadOnly = isEdit && existingData?.status !== 'Created';

  return (
    <>
      <PageHeader
        title={
          isEdit
            ? `Edit ${existingData?.pwoNumber ?? 'PWO'}`
            : 'New Production Work Order'
        }
        action={
          isEdit && existingData?.status ? (
            <Chip
              label={existingData.status}
              color={STATUS_COLORS[existingData.status] || 'default'}
            />
          ) : undefined
        }
      />

      {isReadOnly && (
        <Alert severity="info" sx={{ mb: 2 }}>
          This PWO is in "{existingData?.status}" status and cannot be edited.
        </Alert>
      )}

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ maxWidth: 1100 }}>
        <Grid container spacing={2}>
          {/* Work Order */}
          <Grid size={{ xs: 12, sm: 6 }}>
            <Controller
              name="workOrderId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  disabled={isReadOnly}
                  options={woOptions}
                  getOptionLabel={(o) => `${o.woNumber} — ${o.customerName}`}
                  value={woOptions.find((w) => w.id === field.value) ?? null}
                  onChange={(_, val) => handleWOChange(val)}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Work Order *"
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
          <Grid size={{ xs: 12, sm: 3 }}>
            <Field
              control={control}
              name="date"
              label="Date"
              type="date"
              required
              errors={errors}
              shrink
              disabled={isReadOnly}
            />
          </Grid>

          {/* Shift Allocation */}
          <Grid size={{ xs: 12, sm: 3 }}>
            <FormControl size="small" disabled={isReadOnly}>
              <FormLabel>Shift *</FormLabel>
              <Controller
                name="shiftAllocation"
                control={control}
                render={({ field }) => (
                  <RadioGroup row {...field}>
                    <FormControlLabel value="Day" control={<Radio size="small" />} label="Day" />
                    <FormControlLabel value="Night" control={<Radio size="small" />} label="Night" />
                    <FormControlLabel value="Both" control={<Radio size="small" />} label="Both" />
                  </RadioGroup>
                )}
              />
            </FormControl>
          </Grid>

          {/* Process Type */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="processTypeId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  disabled={isReadOnly}
                  options={processTypeOptions}
                  getOptionLabel={(o) => o.name}
                  value={processTypeOptions.find((pt) => pt.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? 0)}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Process Type *"
                      error={!!errors.processTypeId}
                      helperText={errors.processTypeId?.message}
                    />
                  )}
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
                  disabled={isReadOnly}
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

          {/* Production Unit */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="productionUnitId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  disabled={isReadOnly}
                  options={unitOptions}
                  getOptionLabel={(o) => o.name}
                  value={unitOptions.find((u) => u.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? 0)}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Production Unit *"
                      error={!!errors.productionUnitId}
                      helperText={errors.productionUnitId?.message}
                    />
                  )}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          {/* Start Date */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field
              control={control}
              name="startDate"
              label="Start Date"
              type="date"
              errors={errors}
              shrink
              disabled={isReadOnly}
            />
          </Grid>

          {/* Dispatch Date */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field
              control={control}
              name="dispatchDate"
              label="Dispatch Date"
              type="date"
              errors={errors}
              shrink
              disabled={isReadOnly}
            />
          </Grid>

          {/* Packing Type */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field
              control={control}
              name="packingType"
              label="Packing Type"
              errors={errors}
              disabled={isReadOnly}
            />
          </Grid>

          {/* Line Items Section */}
          <Grid size={12}>
            <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 1, mt: 1 }} />
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                mb: 1,
              }}
            >
              <Typography variant="subtitle1" fontWeight={600}>
                Line Items
              </Typography>
              {!isReadOnly && (
                <Button
                  size="small"
                  startIcon={<Add />}
                  onClick={() => append({ ...defaultLine })}
                >
                  Add Line
                </Button>
              )}
            </Box>

            {errors.lineItems?.root && (
              <Typography color="error" variant="body2" sx={{ mb: 1 }}>
                {errors.lineItems.root.message}
              </Typography>
            )}

            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>#</TableCell>
                    <TableCell sx={{ fontWeight: 600, minWidth: 180 }}>Section *</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>DC No</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Qty *</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Length (mm) *</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Area (sqft)</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Instructions</TableCell>
                    {!isReadOnly && <TableCell />}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {fields.map((field, index) => {
                    const line = watchLines?.[index];
                    const section = sectionOptions.find(
                      (s) => s.id === line?.sectionProfileId,
                    );
                    const perimeterMM = section?.perimeterMM ?? 0;
                    const unitArea =
                      perimeterMM && line?.lengthMM
                        ? (perimeterMM * line.lengthMM) / 1_000_000
                        : 0;
                    const totalSqft =
                      unitArea * (line?.quantity || 0) * 10.7639;

                    return (
                      <TableRow key={field.id}>
                        <TableCell>{index + 1}</TableCell>
                        <TableCell>
                          <Controller
                            name={`lineItems.${index}.sectionProfileId`}
                            control={control}
                            render={({ field: f }) => (
                              <Autocomplete
                                size="small"
                                disabled={isReadOnly}
                                options={sectionOptions}
                                getOptionLabel={(o) => o.name}
                                value={
                                  sectionOptions.find((s) => s.id === f.value) ?? null
                                }
                                onChange={(_, val) => f.onChange(val?.id ?? 0)}
                                renderInput={(params) => (
                                  <TextField
                                    {...params}
                                    error={
                                      !!errors.lineItems?.[index]?.sectionProfileId
                                    }
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
                            name={`lineItems.${index}.customerDCNo`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField
                                {...f}
                                size="small"
                                disabled={isReadOnly}
                                sx={{ width: 100 }}
                              />
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <Controller
                            name={`lineItems.${index}.quantity`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField
                                {...f}
                                size="small"
                                type="number"
                                disabled={isReadOnly}
                                error={!!errors.lineItems?.[index]?.quantity}
                                sx={{ width: 80 }}
                              />
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <Controller
                            name={`lineItems.${index}.lengthMM`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField
                                {...f}
                                size="small"
                                type="number"
                                disabled={isReadOnly}
                                error={!!errors.lineItems?.[index]?.lengthMM}
                                sx={{ width: 100 }}
                              />
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            {totalSqft > 0 ? totalSqft.toFixed(2) : '—'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Controller
                            name={`lineItems.${index}.specialInstructions`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField
                                {...f}
                                size="small"
                                disabled={isReadOnly}
                                sx={{ minWidth: 100 }}
                              />
                            )}
                          />
                        </TableCell>
                        {!isReadOnly && (
                          <TableCell>
                            {fields.length > 1 && (
                              <IconButton
                                size="small"
                                color="error"
                                onClick={() => remove(index)}
                              >
                                <Delete fontSize="small" />
                              </IconButton>
                            )}
                          </TableCell>
                        )}
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </TableContainer>
          </Grid>

          {/* Time Calculation */}
          <Grid size={12}>
            <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 1, mt: 1 }} />
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                mb: 1,
              }}
            >
              <Typography variant="subtitle1" fontWeight={600}>
                Time Calculation
              </Typography>
              <Button
                size="small"
                variant="outlined"
                startIcon={<Calculate />}
                onClick={handleCalcTime}
                disabled={calcMutation.isPending || isReadOnly}
              >
                {calcMutation.isPending ? 'Calculating…' : 'Calculate Time'}
              </Button>
            </Box>

            {timeCalcResult && (
              <Card variant="outlined" sx={{ mb: 2 }}>
                <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
                  <Grid container spacing={2}>
                    <Grid size={{ xs: 4 }}>
                      <Typography variant="caption" color="text.secondary">
                        Pre-Treatment
                      </Typography>
                      <Typography variant="h6">
                        {timeCalcResult.preTreatmentTimeHrs?.toFixed(2)}h
                      </Typography>
                    </Grid>
                    <Grid size={{ xs: 4 }}>
                      <Typography variant="caption" color="text.secondary">
                        Post-Treatment
                      </Typography>
                      <Typography variant="h6">
                        {timeCalcResult.postTreatmentTimeHrs?.toFixed(2)}h
                      </Typography>
                    </Grid>
                    <Grid size={{ xs: 4 }}>
                      <Typography variant="caption" color="text.secondary">
                        Total Time
                      </Typography>
                      <Typography variant="h6" color="primary">
                        {timeCalcResult.totalTimeHrs?.toFixed(2)}h
                      </Typography>
                    </Grid>
                  </Grid>

                  {timeCalcResult.lines?.length > 0 && (
                    <TableContainer sx={{ mt: 1 }}>
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell sx={{ fontWeight: 600 }}>Section</TableCell>
                            <TableCell sx={{ fontWeight: 600 }}>Weight/m</TableCell>
                            <TableCell sx={{ fontWeight: 600 }}>Total Wt (kg)</TableCell>
                            <TableCell sx={{ fontWeight: 600 }}>Loads</TableCell>
                            <TableCell sx={{ fontWeight: 600 }}>Pre-Treat (min)</TableCell>
                            <TableCell sx={{ fontWeight: 600 }}>Post-Treat (min)</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {timeCalcResult.lines.map((lc) => {
                            const sec = sectionOptions.find(
                              (s) => s.id === lc.sectionProfileId,
                            );
                            return (
                              <TableRow key={lc.sectionProfileId}>
                                <TableCell>{sec?.name ?? lc.sectionProfileId}</TableCell>
                                <TableCell>
                                  {lc.calc?.weightPerMtr?.toFixed(2) ?? '—'}
                                </TableCell>
                                <TableCell>
                                  {lc.calc?.totalWeightKg?.toFixed(2) ?? '—'}
                                </TableCell>
                                <TableCell>{lc.calc?.loadsRequired ?? '—'}</TableCell>
                                <TableCell>
                                  {lc.calc?.totalTimePreTreatMins?.toFixed(1) ?? '—'}
                                </TableCell>
                                <TableCell>
                                  {lc.calc?.totalTimePostTreatMins?.toFixed(1) ?? '—'}
                                </TableCell>
                              </TableRow>
                            );
                          })}
                        </TableBody>
                      </Table>
                    </TableContainer>
                  )}
                </CardContent>
              </Card>
            )}

            {/* Show existing time calc data in edit mode */}
            {isEdit && !timeCalcResult && existingData?.preTreatmentTimeHrs != null && (
              <Card variant="outlined" sx={{ mb: 2 }}>
                <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
                  <Grid container spacing={2}>
                    <Grid size={{ xs: 4 }}>
                      <Typography variant="caption" color="text.secondary">
                        Pre-Treatment
                      </Typography>
                      <Typography variant="h6">
                        {existingData.preTreatmentTimeHrs?.toFixed(2)}h
                      </Typography>
                    </Grid>
                    <Grid size={{ xs: 4 }}>
                      <Typography variant="caption" color="text.secondary">
                        Post-Treatment
                      </Typography>
                      <Typography variant="h6">
                        {existingData.postTreatmentTimeHrs?.toFixed(2)}h
                      </Typography>
                    </Grid>
                    <Grid size={{ xs: 4 }}>
                      <Typography variant="caption" color="text.secondary">
                        Total Time
                      </Typography>
                      <Typography variant="h6" color="primary">
                        {existingData.totalTimeHrs?.toFixed(2)}h
                      </Typography>
                    </Grid>
                  </Grid>
                </CardContent>
              </Card>
            )}
          </Grid>

          {/* Special Instructions */}
          <Grid size={12}>
            <Field
              control={control}
              name="specialInstructions"
              label="Special Instructions"
              multiline
              rows={3}
              errors={errors}
              disabled={isReadOnly}
            />
          </Grid>

          {/* Buttons */}
          <Grid size={12}>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 1 }}>
              <Button variant="outlined" onClick={() => navigate('/ppc/work-orders')}>
                Cancel
              </Button>
              {!isReadOnly && (
                <Button
                  type="submit"
                  variant="contained"
                  disabled={
                    isSubmitting ||
                    createMutation.isPending ||
                    updateMutation.isPending
                  }
                >
                  {isSubmitting ||
                  createMutation.isPending ||
                  updateMutation.isPending ? (
                    <CircularProgress size={22} />
                  ) : isEdit ? (
                    'Update PWO'
                  ) : (
                    'Save PWO'
                  )}
                </Button>
              )}
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
