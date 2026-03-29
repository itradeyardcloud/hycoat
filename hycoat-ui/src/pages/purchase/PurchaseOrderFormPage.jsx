import { useEffect } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useForm, Controller, useFieldArray, useWatch } from 'react-hook-form';
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
  TableFooter,
  Paper,
} from '@mui/material';
import { Add, Delete } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  usePurchaseOrder,
  useCreatePurchaseOrder,
  useUpdatePurchaseOrder,
  useGeneratePOPdf,
} from '@/hooks/usePurchaseOrders';
import { usePowderIndent } from '@/hooks/usePowderIndents';
import { useVendorLookup } from '@/hooks/useVendors';
import { usePowderColorLookup } from '@/hooks/usePowderColors';

const lineSchema = z.object({
  powderColorId: z.number({ required_error: 'Required' }).min(1, 'Required'),
  qtyKg: z.coerce.number().min(0.01, 'Min 0.01'),
  ratePerKg: z.coerce.number().min(0.01, 'Min 0.01'),
  requiredByDate: z.string().optional().or(z.literal('')),
});

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  vendorId: z.number({ required_error: 'Vendor is required' }).min(1, 'Vendor is required'),
  powderIndentId: z.number().nullable().optional(),
  notes: z.string().max(1000).optional().or(z.literal('')),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});

const emptyLine = { powderColorId: 0, qtyKg: 0, ratePerKg: 0, requiredByDate: '' };

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  vendorId: 0,
  powderIndentId: null,
  notes: '',
  lines: [{ ...emptyLine }],
};

export default function PurchaseOrderFormPage() {
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const isEdit = !!id;
  const indentIdParam = searchParams.get('indentId');

  const { data: existing, isLoading: loadingExisting } = usePurchaseOrder(id);
  const { data: indentData } = usePowderIndent(indentIdParam);
  const { data: vendors } = useVendorLookup();
  const { data: powderColors } = usePowderColorLookup();
  const createMutation = useCreatePurchaseOrder();
  const updateMutation = useUpdatePurchaseOrder();
  const generatePdfMutation = useGeneratePOPdf();

  const vendorOptions = vendors?.data ?? [];
  const pcOptions = powderColors?.data ?? [];

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues,
  });

  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });
  const watchedLines = useWatch({ control, name: 'lines' });

  // Pre-fill from indent if creating from indent link
  useEffect(() => {
    if (!isEdit && indentData?.data) {
      const indent = indentData.data;
      reset({
        date: new Date().toISOString().split('T')[0],
        vendorId: 0,
        powderIndentId: indent.id,
        notes: `Ref Indent: ${indent.indentNumber}`,
        lines: indent.lines?.length
          ? indent.lines.map((l) => ({
              powderColorId: l.powderColorId || 0,
              qtyKg: l.requiredQtyKg ?? 0,
              ratePerKg: 0,
              requiredByDate: '',
            }))
          : [{ ...emptyLine }],
      });
    }
  }, [indentData, isEdit, reset]);

  // Load existing PO for edit
  useEffect(() => {
    if (existing?.data) {
      const d = existing.data;
      reset({
        date: d.date ? new Date(d.date).toISOString().split('T')[0] : '',
        vendorId: d.vendorId || 0,
        powderIndentId: d.powderIndentId || null,
        notes: d.notes ?? '',
        lines: d.lines?.length
          ? d.lines.map((l) => ({
              powderColorId: l.powderColorId || 0,
              qtyKg: l.qtyKg ?? 0,
              ratePerKg: l.ratePerKg ?? 0,
              requiredByDate: l.requiredByDate
                ? new Date(l.requiredByDate).toISOString().split('T')[0]
                : '',
            }))
          : [{ ...emptyLine }],
      });
    }
  }, [existing, reset]);

  const totalAmount = (watchedLines || []).reduce(
    (sum, l) => sum + (parseFloat(l.qtyKg) || 0) * (parseFloat(l.ratePerKg) || 0),
    0,
  );

  const onSubmit = async (data) => {
    const payload = {
      ...data,
      powderIndentId: data.powderIndentId || null,
      notes: data.notes || null,
      lines: data.lines.map((l) => ({
        ...l,
        requiredByDate: l.requiredByDate || null,
      })),
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    try {
      const result = await mutation.mutateAsync(mutationPayload);
      toast.success(`Purchase order ${isEdit ? 'updated' : 'created'}`);

      // Auto-generate PDF on create
      if (!isEdit && result?.data?.id) {
        try {
          await generatePdfMutation.mutateAsync(result.data.id);
        } catch {
          // PDF generation is non-blocking
        }
      }
      navigate('/purchase/orders');
    } catch (err) {
      const msg =
        err.response?.data?.errors?.[0] ||
        err.response?.data?.message ||
        'Failed to save purchase order';
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
      <PageHeader title={isEdit ? 'Edit Purchase Order' : 'New Purchase Order'} />

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

          {/* Vendor */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="vendorId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={vendorOptions}
                  getOptionLabel={(o) => o.name}
                  value={vendorOptions.find((v) => v.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? 0)}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Vendor"
                      required
                      error={!!errors.vendorId}
                      helperText={errors.vendorId?.message}
                    />
                  )}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
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
                <TableCell sx={{ fontWeight: 600, width: 120 }}>Qty (kg)</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 120 }}>Rate/kg (₹)</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 120 }}>Amount (₹)</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 140 }}>Required By</TableCell>
                <TableCell sx={{ width: 48 }} />
              </TableRow>
            </TableHead>
            <TableBody>
              {fields.map((field, index) => {
                const lineQty = parseFloat(watchedLines?.[index]?.qtyKg) || 0;
                const lineRate = parseFloat(watchedLines?.[index]?.ratePerKg) || 0;
                const lineAmount = lineQty * lineRate;
                return (
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
                        name={`lines.${index}.qtyKg`}
                        control={control}
                        render={({ field: f }) => (
                          <TextField
                            {...f}
                            type="number"
                            size="small"
                            error={!!errors.lines?.[index]?.qtyKg}
                            sx={{ width: 100 }}
                            inputProps={{ step: '0.01' }}
                          />
                        )}
                      />
                    </TableCell>
                    <TableCell>
                      <Controller
                        name={`lines.${index}.ratePerKg`}
                        control={control}
                        render={({ field: f }) => (
                          <TextField
                            {...f}
                            type="number"
                            size="small"
                            error={!!errors.lines?.[index]?.ratePerKg}
                            sx={{ width: 100 }}
                            inputProps={{ step: '0.01' }}
                          />
                        )}
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {lineAmount.toLocaleString('en-IN', { maximumFractionDigits: 2 })}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Controller
                        name={`lines.${index}.requiredByDate`}
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
                );
              })}
            </TableBody>
            <TableFooter>
              <TableRow>
                <TableCell colSpan={3} sx={{ fontWeight: 600, textAlign: 'right' }}>
                  Total:
                </TableCell>
                <TableCell sx={{ fontWeight: 600 }}>
                  ₹{totalAmount.toLocaleString('en-IN', { maximumFractionDigits: 2 })}
                </TableCell>
                <TableCell colSpan={2} />
              </TableRow>
            </TableFooter>
          </Table>
        </TableContainer>

        {/* Buttons */}
        <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
          <Button variant="outlined" onClick={() => navigate('/purchase/orders')}>
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
              'Update PO'
            ) : (
              'Save & Generate PDF'
            )}
          </Button>
        </Box>
      </Box>
    </>
  );
}
