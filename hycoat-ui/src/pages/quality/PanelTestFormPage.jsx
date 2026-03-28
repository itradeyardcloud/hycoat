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
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
} from '@mui/material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import { usePanelTest, useCreatePanelTest, useUpdatePanelTest } from '@/hooks/usePanelTests';
import { useProductionWorkOrderLookup } from '@/hooks/useProductionWorkOrders';

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  productionWorkOrderId: z.number({ required_error: 'PWO is required' }).min(1, 'PWO is required'),
  boilingWaterResult: z.string().optional().or(z.literal('')),
  boilingWaterStatus: z.enum(['Pass', 'Fail']).nullable().optional(),
  impactTestResult: z.string().optional().or(z.literal('')),
  impactTestStatus: z.enum(['Pass', 'Fail']).nullable().optional(),
  conicalMandrelResult: z.string().optional().or(z.literal('')),
  conicalMandrelStatus: z.enum(['Pass', 'Fail']).nullable().optional(),
  pencilHardnessResult: z.string().optional().or(z.literal('')),
  pencilHardnessStatus: z.enum(['Pass', 'Fail']).nullable().optional(),
  remarks: z.string().max(1000).optional().or(z.literal('')),
});

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  productionWorkOrderId: 0,
  boilingWaterResult: '',
  boilingWaterStatus: null,
  impactTestResult: '',
  impactTestStatus: null,
  conicalMandrelResult: '',
  conicalMandrelStatus: null,
  pencilHardnessResult: '',
  pencilHardnessStatus: null,
  remarks: '',
};

const TESTS = [
  { label: 'Boiling Water Test', resultKey: 'boilingWaterResult', statusKey: 'boilingWaterStatus' },
  { label: 'Impact Test', resultKey: 'impactTestResult', statusKey: 'impactTestStatus' },
  { label: 'Conical Mandrel Test', resultKey: 'conicalMandrelResult', statusKey: 'conicalMandrelStatus' },
  { label: 'Pencil Hardness Test', resultKey: 'pencilHardnessResult', statusKey: 'pencilHardnessStatus' },
];

export default function PanelTestFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingExisting } = usePanelTest(id);
  const { data: pwos } = useProductionWorkOrderLookup({ status: 'InProgress' });
  const createMutation = useCreatePanelTest();
  const updateMutation = useUpdatePanelTest();

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

  useEffect(() => {
    if (existing?.data) {
      const d = existing.data;
      reset({
        date: d.date ? new Date(d.date).toISOString().split('T')[0] : '',
        productionWorkOrderId: d.productionWorkOrderId || 0,
        boilingWaterResult: d.boilingWaterResult ?? '',
        boilingWaterStatus: d.boilingWaterStatus ?? null,
        impactTestResult: d.impactTestResult ?? '',
        impactTestStatus: d.impactTestStatus ?? null,
        conicalMandrelResult: d.conicalMandrelResult ?? '',
        conicalMandrelStatus: d.conicalMandrelStatus ?? null,
        pencilHardnessResult: d.pencilHardnessResult ?? '',
        pencilHardnessStatus: d.pencilHardnessStatus ?? null,
        remarks: d.remarks ?? '',
      });
    }
  }, [existing, reset]);

  const onSubmit = async (data) => {
    const payload = {
      ...data,
      boilingWaterResult: data.boilingWaterResult || null,
      boilingWaterStatus: data.boilingWaterStatus || null,
      impactTestResult: data.impactTestResult || null,
      impactTestStatus: data.impactTestStatus || null,
      conicalMandrelResult: data.conicalMandrelResult || null,
      conicalMandrelStatus: data.conicalMandrelStatus || null,
      pencilHardnessResult: data.pencilHardnessResult || null,
      pencilHardnessStatus: data.pencilHardnessStatus || null,
      remarks: data.remarks || null,
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    try {
      await mutation.mutateAsync(mutationPayload);
      toast.success(`Panel test ${isEdit ? 'updated' : 'created'}`);
      navigate('/quality/in-process');
    } catch (err) {
      const msg =
        err.response?.data?.errors?.[0] ||
        err.response?.data?.message ||
        'Failed to save panel test';
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
      <PageHeader title={isEdit ? 'Edit Panel Test' : 'New Panel Test'} />

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

          {/* Test rows */}
          <Grid size={12}>
            <Typography variant="subtitle1" fontWeight={600} sx={{ mt: 1, mb: 1 }}>
              Panel Tests
            </Typography>
            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>Test</TableCell>
                    <TableCell sx={{ fontWeight: 600, minWidth: 180 }}>Result / Observation</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {TESTS.map((test) => (
                    <TableRow key={test.resultKey}>
                      <TableCell>
                        <Typography variant="body2" fontWeight={500}>{test.label}</Typography>
                      </TableCell>
                      <TableCell>
                        <Controller
                          name={test.resultKey}
                          control={control}
                          render={({ field }) => (
                            <TextField {...field} size="small" fullWidth placeholder="Result" />
                          )}
                        />
                      </TableCell>
                      <TableCell>
                        <Controller
                          name={test.statusKey}
                          control={control}
                          render={({ field }) => (
                            <ToggleButtonGroup
                              value={field.value}
                              exclusive
                              onChange={(_, val) => field.onChange(val)}
                              size="small"
                            >
                              <ToggleButton value="Pass" color="success">Pass</ToggleButton>
                              <ToggleButton value="Fail" color="error">Fail</ToggleButton>
                            </ToggleButtonGroup>
                          )}
                        />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
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
                  'Save Panel Test'
                )}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Box>
    </>
  );
}
