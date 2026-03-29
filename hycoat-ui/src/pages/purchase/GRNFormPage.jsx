import { useEffect } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
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
import { useGRN, useCreateGRN, useUpdateGRN } from '@/hooks/useGRNs';
import { usePurchaseOrder } from '@/hooks/usePurchaseOrders';
import { usePowderColorLookup } from '@/hooks/usePowderColors';

const lineSchema = z.object({
  powderColorId: z.number({ required_error: 'Required' }).min(1, 'Required'),
  qtyReceivedKg: z.coerce.number().min(0.01, 'Min 0.01'),
  batchCode: z.string().max(50).optional().or(z.literal('')),
  mfgDate: z.string().optional().or(z.literal('')),
  expiryDate: z.string().optional().or(z.literal('')),
});

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  purchaseOrderId: z.number({ required_error: 'PO is required' }).min(1, 'Purchase Order is required'),
  notes: z.string().max(1000).optional().or(z.literal('')),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});

const emptyLine = { powderColorId: 0, qtyReceivedKg: 0, batchCode: '', mfgDate: '', expiryDate: '' };

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  purchaseOrderId: 0,
  notes: '',
  lines: [{ ...emptyLine }],
};

export default function GRNFormPage() {
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const isEdit = !!id;
  const poIdParam = searchParams.get('poId');

  const { data: existing, isLoading: loadingExisting } = useGRN(id);
  const { data: poData } = usePurchaseOrder(poIdParam || existing?.data?.purchaseOrderId);
  const { data: powderColors } = usePowderColorLookup();
  const createMutation = useCreateGRN();
  const updateMutation = useUpdateGRN();

  const pcOptions = powderColors?.data ?? [];

  const {
    control,
    handleSubmit,
    reset,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues,
  });

  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  // Pre-fill from PO if creating from PO link
  useEffect(() => {
    if (!isEdit && poIdParam && poData?.data) {
      const po = poData.data;
      reset({
        date: new Date().toISOString().split('T')[0],
        purchaseOrderId: po.id,
        notes: `Ref PO: ${po.poNumber}`,
        lines: po.lines?.length
          ? po.lines.map((l) => ({
              powderColorId: l.powderColorId || 0,
              qtyReceivedKg: l.qtyKg ?? 0,
              batchCode: '',
              mfgDate: '',
              expiryDate: '',
            }))
          : [{ ...emptyLine }],
      });
    }
  }, [poData, poIdParam, isEdit, reset]);

  // Load existing GRN for edit
  useEffect(() => {
    if (existing?.data) {
      const d = existing.data;
      reset({
        date: d.date ? new Date(d.date).toISOString().split('T')[0] : '',
        purchaseOrderId: d.purchaseOrderId || 0,
        notes: d.notes ?? '',
        lines: d.lines?.length
          ? d.lines.map((l) => ({
              powderColorId: l.powderColorId || 0,
              qtyReceivedKg: l.qtyReceivedKg ?? 0,
              batchCode: l.batchCode ?? '',
              mfgDate: l.mfgDate ? new Date(l.mfgDate).toISOString().split('T')[0] : '',
              expiryDate: l.expiryDate ? new Date(l.expiryDate).toISOString().split('T')[0] : '',
            }))
          : [{ ...emptyLine }],
      });
    }
  }, [existing, reset]);

  const onSubmit = async (data) => {
    const payload = {
      ...data,
      notes: data.notes || null,
      lines: data.lines.map((l) => ({
        ...l,
        batchCode: l.batchCode || null,
        mfgDate: l.mfgDate || null,
        expiryDate: l.expiryDate || null,
      })),
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    try {
      await mutation.mutateAsync(mutationPayload);
      toast.success(`GRN ${isEdit ? 'updated' : 'created'}`);
      navigate('/purchase/grn');
    } catch (err) {
      const msg =
        err.response?.data?.errors?.[0] ||
        err.response?.data?.message ||
        'Failed to save GRN';
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
      <PageHeader title={isEdit ? 'Edit GRN' : 'New GRN'} />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <Grid container spacing={2} sx={{ maxWidth: 800, mb: 3 }}>
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

          {/* PO Reference */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="purchaseOrderId"
              control={control}
              render={({ field }) => (
                <TextField
                  type="number"
                  label="Purchase Order ID"
                  required
                  fullWidth
                  size="small"
                  value={field.value || ''}
                  onChange={(e) => field.onChange(Number(e.target.value))}
                  error={!!errors.purchaseOrderId}
                  helperText={errors.purchaseOrderId?.message}
                />
              )}
            />
          </Grid>

          {/* Notes */}
          <Grid size={12}>
            <Controller
              name="notes"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Notes"
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
                <TableCell sx={{ fontWeight: 600 }}>Powder Color</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 120 }}>Received (kg)</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 120 }}>Batch Code</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 140 }}>Mfg Date</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 140 }}>Expiry Date</TableCell>
                <TableCell sx={{ width: 48 }} />
              </TableRow>
            </TableHead>
            <TableBody>
              {fields.map((field, index) => (
                <TableRow key={field.id}>
                  <TableCell>
                    <Controller
                      name={`lines.${index}.powderColorId`}
                      control={control}
                      render={({ field: f }) => (
                        <Autocomplete
                          size="small"
                          options={pcOptions}
                          getOptionLabel={(o) => `${o.name}${o.code ? ` (${o.code})` : ''}`}
                          value={pcOptions.find((p) => p.id === f.value) ?? null}
                          onChange={(_, val) => f.onChange(val?.id ?? 0)}
                          renderInput={(params) => (
                            <TextField
                              {...params}
                              error={!!errors.lines?.[index]?.powderColorId}
                              placeholder="Select powder"
                              sx={{ minWidth: 200 }}
                            />
                          )}
                          isOptionEqualToValue={(opt, val) => opt.id === val.id}
                        />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lines.${index}.qtyReceivedKg`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField
                          {...f}
                          type="number"
                          size="small"
                          error={!!errors.lines?.[index]?.qtyReceivedKg}
                          sx={{ width: 100 }}
                          inputProps={{ step: '0.01' }}
                        />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lines.${index}.batchCode`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField {...f} size="small" sx={{ width: 100 }} />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lines.${index}.mfgDate`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField
                          {...f}
                          type="date"
                          size="small"
                          sx={{ width: 140 }}
                          slotProps={{ inputLabel: { shrink: true } }}
                        />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lines.${index}.expiryDate`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField
                          {...f}
                          type="date"
                          size="small"
                          sx={{ width: 140 }}
                          slotProps={{ inputLabel: { shrink: true } }}
                        />
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
          <Button variant="outlined" onClick={() => navigate('/purchase/grn')}>
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
              'Update GRN'
            ) : (
              'Save GRN'
            )}
          </Button>
        </Box>
      </Box>
    </>
  );
}
