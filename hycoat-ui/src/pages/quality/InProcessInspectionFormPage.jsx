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
  Typography,
  ToggleButton,
  ToggleButtonGroup,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Chip,
} from '@mui/material';
import { Add, Delete } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  useInProcessInspection,
  useCreateInProcessInspection,
  useUpdateInProcessInspection,
} from '@/hooks/useInProcessInspections';
import { useProductionWorkOrderLookup } from '@/hooks/useProductionWorkOrders';
import { useSectionProfileLookup } from '@/hooks/useSectionProfiles';

const DFT_MIN = 60;
const DFT_MAX = 80;

const dftReadingSchema = z.object({
  sectionProfileId: z.number().nullable().optional(),
  s1: z.coerce.number().min(0).max(200).nullable().optional(),
  s2: z.coerce.number().min(0).max(200).nullable().optional(),
  s3: z.coerce.number().min(0).max(200).nullable().optional(),
  s4: z.coerce.number().min(0).max(200).nullable().optional(),
  s5: z.coerce.number().min(0).max(200).nullable().optional(),
  s6: z.coerce.number().min(0).max(200).nullable().optional(),
  s7: z.coerce.number().min(0).max(200).nullable().optional(),
  s8: z.coerce.number().min(0).max(200).nullable().optional(),
  s9: z.coerce.number().min(0).max(200).nullable().optional(),
  s10: z.coerce.number().min(0).max(200).nullable().optional(),
});

const testResultSchema = z.object({
  testType: z.string().min(1, 'Required'),
  instrumentName: z.string().optional().or(z.literal('')),
  instrumentMake: z.string().optional().or(z.literal('')),
  instrumentModel: z.string().optional().or(z.literal('')),
  calibrationDate: z.string().optional().or(z.literal('')),
  referenceStandard: z.string().optional().or(z.literal('')),
  standardLimit: z.string().optional().or(z.literal('')),
  result: z.string().min(1, 'Required'),
  status: z.enum(['Pass', 'Fail'], { required_error: 'Required' }),
  remarks: z.string().optional().or(z.literal('')),
});

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  time: z.string().min(1, 'Time is required'),
  productionWorkOrderId: z.number({ required_error: 'PWO is required' }).min(1, 'PWO is required'),
  remarks: z.string().max(1000).optional().or(z.literal('')),
  dftReadings: z.array(dftReadingSchema),
  testResults: z.array(testResultSchema),
});

const defaultDFTReading = {
  sectionProfileId: null,
  s1: null, s2: null, s3: null, s4: null, s5: null,
  s6: null, s7: null, s8: null, s9: null, s10: null,
};

const defaultTestResult = {
  testType: '',
  instrumentName: '',
  instrumentMake: '',
  instrumentModel: '',
  calibrationDate: '',
  referenceStandard: '',
  standardLimit: '',
  result: '',
  status: 'Pass',
  remarks: '',
};

function now() {
  const d = new Date();
  return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
}

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  time: now(),
  productionWorkOrderId: 0,
  remarks: '',
  dftReadings: [{ ...defaultDFTReading }],
  testResults: [],
};

function computeDFTStats(reading) {
  const vals = [reading.s1, reading.s2, reading.s3, reading.s4, reading.s5,
    reading.s6, reading.s7, reading.s8, reading.s9, reading.s10]
    .filter((v) => v != null && v !== '' && !isNaN(v))
    .map(Number);
  if (vals.length === 0) return { min: null, max: null, avg: null, withinSpec: true };
  const min = Math.min(...vals);
  const max = Math.max(...vals);
  const avg = +(vals.reduce((a, b) => a + b, 0) / vals.length).toFixed(1);
  const withinSpec = vals.every((v) => v >= DFT_MIN && v <= DFT_MAX);
  return { min, max, avg, withinSpec };
}

export default function InProcessInspectionFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingExisting } = useInProcessInspection(id);
  const { data: pwos } = useProductionWorkOrderLookup({ status: 'InProgress' });
  const { data: sections } = useSectionProfileLookup();
  const createMutation = useCreateInProcessInspection();
  const updateMutation = useUpdateInProcessInspection();

  const pwoOptions = pwos?.data ?? [];
  const sectionOptions = sections?.data ?? [];

  const {
    control,
    handleSubmit,
    reset,
    watch,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues,
  });

  const {
    fields: dftFields,
    append: appendDFT,
    remove: removeDFT,
  } = useFieldArray({ control, name: 'dftReadings' });

  const {
    fields: testFields,
    append: appendTest,
    remove: removeTest,
  } = useFieldArray({ control, name: 'testResults' });

  const watchDFT = watch('dftReadings');

  useEffect(() => {
    if (existing?.data) {
      const d = existing.data;
      const timeParts = d.time ? d.time.split(':') : null;
      reset({
        date: d.date ? new Date(d.date).toISOString().split('T')[0] : '',
        time: timeParts ? `${timeParts[0]}:${timeParts[1]}` : now(),
        productionWorkOrderId: d.productionWorkOrderId || 0,
        remarks: d.remarks ?? '',
        dftReadings: d.dftReadings?.length > 0
          ? d.dftReadings.map((r) => ({
            sectionProfileId: r.sectionProfileId ?? null,
            s1: r.s1, s2: r.s2, s3: r.s3, s4: r.s4, s5: r.s5,
            s6: r.s6, s7: r.s7, s8: r.s8, s9: r.s9, s10: r.s10,
          }))
          : [{ ...defaultDFTReading }],
        testResults: d.testResults?.map((t) => ({
          testType: t.testType ?? '',
          instrumentName: t.instrumentName ?? '',
          instrumentMake: t.instrumentMake ?? '',
          instrumentModel: t.instrumentModel ?? '',
          calibrationDate: t.calibrationDate ? new Date(t.calibrationDate).toISOString().split('T')[0] : '',
          referenceStandard: t.referenceStandard ?? '',
          standardLimit: t.standardLimit ?? '',
          result: t.result ?? '',
          status: t.status ?? 'Pass',
          remarks: t.remarks ?? '',
        })) ?? [],
      });
    }
  }, [existing, reset]);

  const onSubmit = async (data) => {
    const payload = {
      ...data,
      time: data.time + ':00',
      remarks: data.remarks || null,
      dftReadings: data.dftReadings.map((r) => ({
        sectionProfileId: r.sectionProfileId || null,
        s1: r.s1 ?? null, s2: r.s2 ?? null, s3: r.s3 ?? null, s4: r.s4 ?? null, s5: r.s5 ?? null,
        s6: r.s6 ?? null, s7: r.s7 ?? null, s8: r.s8 ?? null, s9: r.s9 ?? null, s10: r.s10 ?? null,
      })),
      testResults: data.testResults.map((t) => ({
        ...t,
        calibrationDate: t.calibrationDate || null,
        instrumentName: t.instrumentName || null,
        instrumentMake: t.instrumentMake || null,
        instrumentModel: t.instrumentModel || null,
        referenceStandard: t.referenceStandard || null,
        standardLimit: t.standardLimit || null,
        remarks: t.remarks || null,
      })),
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    try {
      await mutation.mutateAsync(mutationPayload);
      toast.success(`Inspection ${isEdit ? 'updated' : 'created'}`);
      navigate('/quality/in-process');
    } catch (err) {
      const msg =
        err.response?.data?.errors?.[0] ||
        err.response?.data?.message ||
        'Failed to save inspection';
      toast.error(msg);
    }
  };

  const dftStats = useMemo(
    () => (watchDFT || []).map((r) => computeDFTStats(r)),
    [watchDFT],
  );

  if (isEdit && loadingExisting) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <>
      <PageHeader title={isEdit ? 'Edit In-Process Inspection' : 'New In-Process Inspection'} />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ maxWidth: 1200 }}>
        <Grid container spacing={2}>
          {/* PWO Autocomplete */}
          <Grid size={{ xs: 12, sm: 6 }}>
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
          <Grid size={{ xs: 6, sm: 3 }}>
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

          {/* Time */}
          <Grid size={{ xs: 6, sm: 3 }}>
            <Controller
              name="time"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  type="time"
                  label="Time"
                  required
                  fullWidth
                  size="small"
                  error={!!errors.time}
                  helperText={errors.time?.message}
                  slotProps={{ inputLabel: { shrink: true } }}
                />
              )}
            />
          </Grid>

          {/* DFT Readings Section */}
          <Grid size={12}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mt: 2, mb: 1 }}>
              <Typography variant="subtitle1" fontWeight={600}>
                DFT Readings (µm) — Spec: {DFT_MIN}–{DFT_MAX}
              </Typography>
              <Button size="small" startIcon={<Add />} onClick={() => appendDFT({ ...defaultDFTReading })}>
                Add Row
              </Button>
            </Box>

            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>#</TableCell>
                    <TableCell sx={{ fontWeight: 600, minWidth: 150 }}>Section</TableCell>
                    {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((n) => (
                      <TableCell key={n} sx={{ fontWeight: 600 }} align="center">S{n}</TableCell>
                    ))}
                    <TableCell sx={{ fontWeight: 600 }} align="center">Min</TableCell>
                    <TableCell sx={{ fontWeight: 600 }} align="center">Max</TableCell>
                    <TableCell sx={{ fontWeight: 600 }} align="center">Avg</TableCell>
                    <TableCell sx={{ fontWeight: 600 }} align="center">Spec</TableCell>
                    <TableCell />
                  </TableRow>
                </TableHead>
                <TableBody>
                  {dftFields.map((field, index) => {
                    const stats = dftStats[index] || {};
                    return (
                      <TableRow key={field.id}>
                        <TableCell>{index + 1}</TableCell>
                        <TableCell>
                          <Controller
                            name={`dftReadings.${index}.sectionProfileId`}
                            control={control}
                            render={({ field: f }) => (
                              <Autocomplete
                                size="small"
                                options={sectionOptions}
                                getOptionLabel={(o) => o.name}
                                value={sectionOptions.find((s) => s.id === f.value) ?? null}
                                onChange={(_, val) => f.onChange(val?.id ?? null)}
                                renderInput={(params) => <TextField {...params} sx={{ minWidth: 120 }} />}
                                isOptionEqualToValue={(opt, val) => opt.id === val.id}
                              />
                            )}
                          />
                        </TableCell>
                        {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((n) => (
                          <TableCell key={n}>
                            <Controller
                              name={`dftReadings.${index}.s${n}`}
                              control={control}
                              render={({ field: f }) => (
                                <TextField
                                  value={f.value ?? ''}
                                  onChange={(e) => f.onChange(e.target.value === '' ? null : Number(e.target.value))}
                                  size="small"
                                  type="number"
                                  sx={{ width: 65 }}
                                  slotProps={{ htmlInput: { min: 0, max: 200, step: 0.1 } }}
                                />
                              )}
                            />
                          </TableCell>
                        ))}
                        <TableCell align="center">
                          <Typography variant="body2">{stats.min ?? '—'}</Typography>
                        </TableCell>
                        <TableCell align="center">
                          <Typography variant="body2">{stats.max ?? '—'}</Typography>
                        </TableCell>
                        <TableCell align="center">
                          <Typography variant="body2" fontWeight={600}>{stats.avg ?? '—'}</Typography>
                        </TableCell>
                        <TableCell align="center">
                          {stats.avg != null && (
                            <Chip
                              label={stats.withinSpec ? 'OK' : 'Fail'}
                              size="small"
                              color={stats.withinSpec ? 'success' : 'error'}
                            />
                          )}
                        </TableCell>
                        <TableCell>
                          {dftFields.length > 1 && (
                            <IconButton size="small" color="error" onClick={() => removeDFT(index)}>
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

          {/* Quality Tests Section */}
          <Grid size={12}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mt: 2, mb: 1 }}>
              <Typography variant="subtitle1" fontWeight={600}>
                Quality Tests
              </Typography>
              <Button size="small" startIcon={<Add />} onClick={() => appendTest({ ...defaultTestResult })}>
                Add Test
              </Button>
            </Box>

            {testFields.length > 0 && (
              <TableContainer component={Paper} variant="outlined">
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell sx={{ fontWeight: 600 }}>#</TableCell>
                      <TableCell sx={{ fontWeight: 600, minWidth: 140 }}>Test Type</TableCell>
                      <TableCell sx={{ fontWeight: 600, minWidth: 120 }}>Instrument</TableCell>
                      <TableCell sx={{ fontWeight: 600, minWidth: 100 }}>Std / Limit</TableCell>
                      <TableCell sx={{ fontWeight: 600, minWidth: 100 }}>Result</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Remarks</TableCell>
                      <TableCell />
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {testFields.map((field, index) => (
                      <TableRow key={field.id}>
                        <TableCell>{index + 1}</TableCell>
                        <TableCell>
                          <Controller
                            name={`testResults.${index}.testType`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField
                                {...f}
                                size="small"
                                placeholder="e.g. DFT, MEK"
                                error={!!errors.testResults?.[index]?.testType}
                                sx={{ minWidth: 120 }}
                              />
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <Controller
                            name={`testResults.${index}.instrumentName`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField {...f} size="small" placeholder="Instrument" sx={{ minWidth: 100 }} />
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <Controller
                            name={`testResults.${index}.standardLimit`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField {...f} size="small" placeholder="Std limit" sx={{ minWidth: 90 }} />
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <Controller
                            name={`testResults.${index}.result`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField
                                {...f}
                                size="small"
                                placeholder="Result"
                                error={!!errors.testResults?.[index]?.result}
                                sx={{ minWidth: 90 }}
                              />
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <Controller
                            name={`testResults.${index}.status`}
                            control={control}
                            render={({ field: f }) => (
                              <ToggleButtonGroup
                                value={f.value}
                                exclusive
                                onChange={(_, val) => { if (val) f.onChange(val); }}
                                size="small"
                              >
                                <ToggleButton value="Pass" color="success">Pass</ToggleButton>
                                <ToggleButton value="Fail" color="error">Fail</ToggleButton>
                              </ToggleButtonGroup>
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <Controller
                            name={`testResults.${index}.remarks`}
                            control={control}
                            render={({ field: f }) => (
                              <TextField {...f} size="small" sx={{ minWidth: 90 }} />
                            )}
                          />
                        </TableCell>
                        <TableCell>
                          <IconButton size="small" color="error" onClick={() => removeTest(index)}>
                            <Delete fontSize="small" />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
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
              <Button variant="outlined" onClick={() => navigate('/quality/in-process')}>
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

