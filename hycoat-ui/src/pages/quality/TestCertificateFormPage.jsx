import { useEffect } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
  Box,
  Button,
  TextField,
  Grid,
  CircularProgress,
  Typography,
  MenuItem,
} from '@mui/material';
import { PictureAsPdf } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  useTestCertificate,
  useCreateTestCertificate,
  useUpdateTestCertificate,
  useGenerateTestCertificatePdf,
} from '@/hooks/useTestCertificates';
import { useFinalInspection } from '@/hooks/useFinalInspections';

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  finalInspectionId: z.coerce.number().min(1, 'Final Inspection is required'),
  customerId: z.coerce.number().min(1, 'Customer is required'),
  workOrderId: z.coerce.number().min(1, 'Work Order is required'),
  productCode: z.string().optional().or(z.literal('')),
  projectName: z.string().optional().or(z.literal('')),
  lotQuantity: z.coerce.number().min(1, 'Min 1'),
  warranty: z.string().optional().or(z.literal('')),
  substrateResult: z.string().optional().or(z.literal('')),
  bakingTempResult: z.string().optional().or(z.literal('')),
  bakingTimeResult: z.string().optional().or(z.literal('')),
  colorResult: z.string().optional().or(z.literal('')),
  dftResult: z.string().optional().or(z.literal('')),
  mekResult: z.string().optional().or(z.literal('')),
  crossCutResult: z.string().optional().or(z.literal('')),
  conicalMandrelResult: z.string().optional().or(z.literal('')),
  boilingWaterResult: z.string().optional().or(z.literal('')),
});

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  finalInspectionId: 0,
  customerId: 0,
  workOrderId: 0,
  productCode: '',
  projectName: '',
  lotQuantity: 0,
  warranty: '',
  substrateResult: '',
  bakingTempResult: '',
  bakingTimeResult: '',
  colorResult: '',
  dftResult: '',
  mekResult: '',
  crossCutResult: '',
  conicalMandrelResult: '',
  boilingWaterResult: '',
};

export default function TestCertificateFormPage() {
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const finalInspectionIdFromQuery = searchParams.get('finalInspectionId');

  const { data: existing, isLoading: loadingExisting } = useTestCertificate(id);
  const { data: fir } = useFinalInspection(finalInspectionIdFromQuery);
  const createMutation = useCreateTestCertificate();
  const updateMutation = useUpdateTestCertificate();
  const generatePdfMutation = useGenerateTestCertificatePdf();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues,
  });

  // Pre-fill from final inspection (new mode)
  useEffect(() => {
    if (!isEdit && fir?.data) {
      const d = fir.data;
      reset({
        date: new Date().toISOString().split('T')[0],
        finalInspectionId: d.id,
        customerId: d.customerId || 0,
        workOrderId: d.workOrderId || 0,
        productCode: '',
        projectName: '',
        lotQuantity: d.lotQuantity ?? 0,
        warranty: '',
        substrateResult: '',
        bakingTempResult: '',
        bakingTimeResult: '',
        colorResult: '',
        dftResult: '',
        mekResult: '',
        crossCutResult: '',
        conicalMandrelResult: '',
        boilingWaterResult: '',
      });
    }
  }, [fir, isEdit, reset]);

  // Populate form for edit
  useEffect(() => {
    if (existing?.data) {
      const d = existing.data;
      reset({
        date: d.date ? new Date(d.date).toISOString().split('T')[0] : '',
        finalInspectionId: d.finalInspectionId || 0,
        customerId: d.customerId || 0,
        workOrderId: d.workOrderId || 0,
        productCode: d.productCode ?? '',
        projectName: d.projectName ?? '',
        lotQuantity: d.lotQuantity ?? 0,
        warranty: d.warranty ?? '',
        substrateResult: d.substrateResult ?? '',
        bakingTempResult: d.bakingTempResult ?? '',
        bakingTimeResult: d.bakingTimeResult ?? '',
        colorResult: d.colorResult ?? '',
        dftResult: d.dftResult ?? '',
        mekResult: d.mekResult ?? '',
        crossCutResult: d.crossCutResult ?? '',
        conicalMandrelResult: d.conicalMandrelResult ?? '',
        boilingWaterResult: d.boilingWaterResult ?? '',
      });
    }
  }, [existing, reset]);

  const onSubmit = async (data) => {
    const payload = {
      ...data,
      productCode: data.productCode || null,
      projectName: data.projectName || null,
      warranty: data.warranty || null,
      substrateResult: data.substrateResult || null,
      bakingTempResult: data.bakingTempResult || null,
      bakingTimeResult: data.bakingTimeResult || null,
      colorResult: data.colorResult || null,
      dftResult: data.dftResult || null,
      mekResult: data.mekResult || null,
      crossCutResult: data.crossCutResult || null,
      conicalMandrelResult: data.conicalMandrelResult || null,
      boilingWaterResult: data.boilingWaterResult || null,
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    try {
      await mutation.mutateAsync(mutationPayload);
      toast.success(`Test certificate ${isEdit ? 'updated' : 'created'}`);
      navigate('/quality/test-certificates');
    } catch (err) {
      const msg =
        err.response?.data?.errors?.[0] ||
        err.response?.data?.message ||
        'Failed to save test certificate';
      toast.error(msg);
    }
  };

  const handleGeneratePdf = async () => {
    try {
      await generatePdfMutation.mutateAsync(Number(id));
      toast.success('PDF generated');
    } catch (err) {
      const msg = err.response?.data?.message || 'Failed to generate PDF';
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

  const TEST_RESULTS = [
    { label: 'Substrate Pre-treatment', name: 'substrateResult', placeholder: 'e.g. 7-Tank' },
    { label: 'Baking Temperature', name: 'bakingTempResult', placeholder: 'e.g. 200°C' },
    { label: 'Baking Time', name: 'bakingTimeResult', placeholder: 'e.g. 10 min' },
    { label: 'Color / Shade', name: 'colorResult', placeholder: 'e.g. RAL 9010' },
    { label: 'DFT (µm)', name: 'dftResult', placeholder: 'e.g. 60-80' },
    { label: 'MEK Rub Test', name: 'mekResult', placeholder: 'e.g. Pass / >100 rubs' },
    { label: 'Cross-Cut Test', name: 'crossCutResult', placeholder: 'e.g. 0B-5B' },
    { label: 'Conical Mandrel', name: 'conicalMandrelResult', placeholder: 'e.g. No crack' },
    { label: 'Boiling Water Test', name: 'boilingWaterResult', placeholder: 'e.g. No blister' },
  ];

  return (
    <>
      <PageHeader
        title={isEdit ? 'Edit Test Certificate' : 'New Test Certificate'}
        action={
          isEdit && (
            <Button
              variant="outlined"
              startIcon={<PictureAsPdf />}
              onClick={handleGeneratePdf}
              disabled={generatePdfMutation.isPending}
            >
              {generatePdfMutation.isPending ? 'Generating…' : 'Generate PDF'}
            </Button>
          )
        }
      />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ maxWidth: 900 }}>
        <Grid container spacing={2}>
          {/* Hidden IDs */}
          <input type="hidden" {...control.register('finalInspectionId')} />
          <input type="hidden" {...control.register('customerId')} />
          <input type="hidden" {...control.register('workOrderId')} />

          {/* Meta info */}
          {(existing?.data || fir?.data) && (
            <Grid size={12}>
              <Typography variant="body2" color="text.secondary">
                {existing?.data
                  ? `Certificate: ${existing.data.certificateNumber} | Customer: ${existing.data.customerName} | WO: ${existing.data.workOrderNumber}`
                  : `Final Inspection: ${fir.data.inspectionNumber} | PWO: ${fir.data.pwoNumber} | Customer: ${fir.data.customerName}`}
              </Typography>
            </Grid>
          )}

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

          {/* Product Code */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="productCode"
              control={control}
              render={({ field }) => (
                <TextField {...field} label="Product Code" fullWidth size="small" />
              )}
            />
          </Grid>

          {/* Project Name */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="projectName"
              control={control}
              render={({ field }) => (
                <TextField {...field} label="Project Name" fullWidth size="small" />
              )}
            />
          </Grid>

          {/* Lot Quantity */}
          <Grid size={{ xs: 12, sm: 4 }}>
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

          {/* Warranty */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="warranty"
              control={control}
              render={({ field }) => (
                <TextField {...field} select label="Warranty" fullWidth size="small">
                  <MenuItem value="">None</MenuItem>
                  <MenuItem value="15 Years">15 Years</MenuItem>
                  <MenuItem value="25 Years">25 Years</MenuItem>
                </TextField>
              )}
            />
          </Grid>

          {/* Test Results */}
          <Grid size={12}>
            <Typography variant="subtitle1" fontWeight={600} sx={{ mt: 2, mb: 1 }}>
              Test Results
            </Typography>
          </Grid>

          {TEST_RESULTS.map((test) => (
            <Grid key={test.name} size={{ xs: 12, sm: 6 }}>
              <Controller
                name={test.name}
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={test.label}
                    placeholder={test.placeholder}
                    fullWidth
                    size="small"
                  />
                )}
              />
            </Grid>
          ))}

          {/* Buttons */}
          <Grid size={12}>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 1 }}>
              <Button variant="outlined" onClick={() => navigate('/quality/test-certificates')}>
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
                  'Save Certificate'
                )}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Box>
    </>
  );
}
