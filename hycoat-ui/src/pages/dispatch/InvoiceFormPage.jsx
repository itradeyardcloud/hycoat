import { useEffect, useState } from 'react';
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
  Chip,
  FormControlLabel,
  Switch,
  Divider,
  Card,
  CardContent,
} from '@mui/material';
import { Add, Delete, AutoFixHigh } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  useInvoice,
  useCreateInvoice,
  useUpdateInvoice,
  useInvoiceAutoFill,
} from '@/hooks/useInvoices';
import { useWorkOrderLookup } from '@/hooks/useMaterialInwards';
import { useCustomerLookup } from '@/hooks/useCustomers';
import { useSectionProfileLookup } from '@/hooks/useSectionProfiles';
import { calculateAreaSFT, calculateLineAmount } from '@/utils/areaCalculator';
import { formatCurrency } from '@/utils/formatters';

const lineSchema = z.object({
  sectionProfileId: z.number({ required_error: 'Required' }).min(1, 'Required'),
  perimeterMM: z.coerce.number().min(0.01, 'Required'),
  lengthMM: z.coerce.number().min(1, 'Required'),
  quantity: z.coerce.number().min(1, 'Min 1'),
  ratePerSFT: z.coerce.number().min(0.01, 'Required'),
});

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  workOrderId: z.number({ required_error: 'WO is required' }).min(1, 'Work Order is required'),
  customerId: z.number({ required_error: 'Customer is required' }).min(1, 'Customer is required'),
  isInterState: z.boolean(),
  cgstRate: z.coerce.number().min(0).max(28),
  sgstRate: z.coerce.number().min(0).max(28),
  igstRate: z.coerce.number().min(0).max(28),
  transportCharges: z.coerce.number().min(0).optional(),
  packingCharges: z.coerce.number().min(0).optional(),
  otherCharges: z.coerce.number().min(0).optional(),
  remarks: z.string().max(1000).optional().or(z.literal('')),
  lineItems: z.array(lineSchema).min(1, 'At least one line item is required'),
});

const emptyLine = { sectionProfileId: 0, perimeterMM: 0, lengthMM: 0, quantity: 0, ratePerSFT: 0 };

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  workOrderId: 0,
  customerId: 0,
  isInterState: false,
  cgstRate: 9,
  sgstRate: 9,
  igstRate: 18,
  transportCharges: 0,
  packingCharges: 0,
  otherCharges: 0,
  remarks: '',
  lineItems: [{ ...emptyLine }],
};

const STATUS_COLORS = {
  Draft: 'default',
  Finalized: 'info',
  Sent: 'primary',
  Paid: 'success',
};

export default function InvoiceFormPage() {
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const autoFillWoId = searchParams.get('woId');
  const [autoFillTriggered, setAutoFillTriggered] = useState(false);

  const { data: existing, isLoading: loadingExisting } = useInvoice(id);
  const { data: wos } = useWorkOrderLookup();
  const { data: customers } = useCustomerLookup();
  const { data: sectionProfiles } = useSectionProfileLookup();
  const { data: autoFillData } = useInvoiceAutoFill(autoFillTriggered ? autoFillWoId : null);
  const createMutation = useCreateInvoice();
  const updateMutation = useUpdateInvoice();

  const woOptions = wos?.data ?? [];
  const customerOptions = customers?.data ?? [];
  const spOptions = sectionProfiles?.data ?? [];

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

  const { fields, append, remove, replace } = useFieldArray({ control, name: 'lineItems' });
  const lineItems = useWatch({ control, name: 'lineItems' });
  const isInterState = useWatch({ control, name: 'isInterState' });
  const cgstRate = useWatch({ control, name: 'cgstRate' });
  const sgstRate = useWatch({ control, name: 'sgstRate' });
  const igstRate = useWatch({ control, name: 'igstRate' });
  const transportCharges = useWatch({ control, name: 'transportCharges' });
  const packingCharges = useWatch({ control, name: 'packingCharges' });
  const otherCharges = useWatch({ control, name: 'otherCharges' });

  // Calculate totals
  const lineTotals = (lineItems || []).map((l) => {
    const area = calculateAreaSFT(l.perimeterMM, l.lengthMM, l.quantity);
    const amount = calculateLineAmount(area, l.ratePerSFT);
    return { area, amount };
  });
  const totalSFT = lineTotals.reduce((s, l) => s + l.area, 0);
  const subTotal = lineTotals.reduce((s, l) => s + l.amount, 0);
  const addCharges = (transportCharges || 0) + (packingCharges || 0) + (otherCharges || 0);
  const taxableAmount = subTotal + addCharges;
  const cgstAmount = !isInterState ? (taxableAmount * (cgstRate || 0)) / 100 : 0;
  const sgstAmount = !isInterState ? (taxableAmount * (sgstRate || 0)) / 100 : 0;
  const igstAmount = isInterState ? (taxableAmount * (igstRate || 0)) / 100 : 0;
  const totalBeforeRound = taxableAmount + cgstAmount + sgstAmount + igstAmount;
  const grandTotal = Math.round(totalBeforeRound);
  const roundOff = grandTotal - totalBeforeRound;

  // Auto-fill from WO query param
  useEffect(() => {
    if (autoFillWoId && !isEdit && !autoFillTriggered) {
      setAutoFillTriggered(true);
    }
  }, [autoFillWoId, isEdit, autoFillTriggered]);

  useEffect(() => {
    if (autoFillData?.data && !isEdit) {
      const af = autoFillData.data;
      setValue('workOrderId', af.workOrderId || 0);
      setValue('customerId', af.customerId || 0);
      if (af.lines?.length) {
        replace(
          af.lines.map((l) => ({
            sectionProfileId: l.sectionProfileId || 0,
            perimeterMM: l.perimeterMM || 0,
            lengthMM: l.lengthMM || 0,
            quantity: l.quantity || 0,
            ratePerSFT: 0,
          })),
        );
      }
    }
  }, [autoFillData, isEdit, setValue, replace]);

  useEffect(() => {
    if (existing?.data) {
      const d = existing.data;
      reset({
        date: d.date ? new Date(d.date).toISOString().split('T')[0] : '',
        workOrderId: d.workOrderId || 0,
        customerId: d.customerId || 0,
        isInterState: d.isInterState ?? false,
        cgstRate: d.cgstRate ?? 9,
        sgstRate: d.sgstRate ?? 9,
        igstRate: d.igstRate ?? 18,
        transportCharges: d.transportCharges ?? 0,
        packingCharges: d.packingCharges ?? 0,
        otherCharges: d.otherCharges ?? 0,
        remarks: d.remarks ?? '',
        lineItems: d.lineItems?.length
          ? d.lineItems.map((l) => ({
              sectionProfileId: l.sectionProfileId || 0,
              perimeterMM: l.perimeterMM || 0,
              lengthMM: l.lengthMM || 0,
              quantity: l.quantity ?? 0,
              ratePerSFT: l.ratePerSFT ?? 0,
            }))
          : [{ ...emptyLine }],
      });
    }
  }, [existing, reset]);

  const onSubmit = async (data) => {
    const payload = {
      ...data,
      transportCharges: data.transportCharges || 0,
      packingCharges: data.packingCharges || 0,
      otherCharges: data.otherCharges || 0,
      remarks: data.remarks || null,
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    try {
      await mutation.mutateAsync(mutationPayload);
      toast.success(`Invoice ${isEdit ? 'updated' : 'created'}`);
      navigate('/dispatch/invoices');
    } catch (err) {
      const msg =
        err.response?.data?.errors?.[0] ||
        err.response?.data?.message ||
        'Failed to save invoice';
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

  const existingData = existing?.data;

  return (
    <>
      <PageHeader
        title={isEdit ? `Edit ${existingData?.invoiceNumber ?? 'Invoice'}` : 'New Invoice'}
        action={
          isEdit && existingData?.status ? (
            <Chip
              label={existingData.status}
              color={STATUS_COLORS[existingData.status] || 'default'}
            />
          ) : undefined
        }
      />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <Grid container spacing={2} sx={{ maxWidth: 900, mb: 3 }}>
          {/* Work Order */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="workOrderId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={woOptions}
                  getOptionLabel={(o) => o.name}
                  value={woOptions.find((w) => w.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? 0)}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Work Order"
                      required
                      error={!!errors.workOrderId}
                      helperText={errors.workOrderId?.message}
                    />
                  )}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          {/* Customer */}
          <Grid size={{ xs: 12, sm: 4 }}>
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

          {/* Inter-State Toggle */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="isInterState"
              control={control}
              render={({ field }) => (
                <FormControlLabel
                  control={<Switch checked={field.value} onChange={field.onChange} />}
                  label="Inter-State (IGST)"
                />
              )}
            />
          </Grid>

          {/* GST Rates */}
          {!isInterState ? (
            <>
              <Grid size={{ xs: 6, sm: 4 }}>
                <Controller
                  name="cgstRate"
                  control={control}
                  render={({ field }) => (
                    <TextField {...field} type="number" label="CGST %" fullWidth size="small" />
                  )}
                />
              </Grid>
              <Grid size={{ xs: 6, sm: 4 }}>
                <Controller
                  name="sgstRate"
                  control={control}
                  render={({ field }) => (
                    <TextField {...field} type="number" label="SGST %" fullWidth size="small" />
                  )}
                />
              </Grid>
            </>
          ) : (
            <Grid size={{ xs: 12, sm: 4 }}>
              <Controller
                name="igstRate"
                control={control}
                render={({ field }) => (
                  <TextField {...field} type="number" label="IGST %" fullWidth size="small" />
                )}
              />
            </Grid>
          )}

          {/* Additional Charges */}
          <Grid size={12}>
            <Divider sx={{ my: 1 }} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="transportCharges"
              control={control}
              render={({ field }) => (
                <TextField {...field} type="number" label="Transport Charges" fullWidth size="small" />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="packingCharges"
              control={control}
              render={({ field }) => (
                <TextField {...field} type="number" label="Packing Charges" fullWidth size="small" />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="otherCharges"
              control={control}
              render={({ field }) => (
                <TextField {...field} type="number" label="Other Charges" fullWidth size="small" />
              )}
            />
          </Grid>

          {/* Remarks */}
          <Grid size={12}>
            <Controller
              name="remarks"
              control={control}
              render={({ field }) => (
                <TextField {...field} label="Remarks" fullWidth size="small" multiline minRows={2} />
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
        {errors.lineItems?.root && (
          <Typography color="error" variant="caption" sx={{ mb: 1, display: 'block' }}>
            {errors.lineItems.root.message}
          </Typography>
        )}

        <TableContainer component={Paper} variant="outlined" sx={{ mb: 3, overflowX: 'auto' }}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Section Profile</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 100 }}>Perimeter (mm)</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 100 }}>Length (mm)</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 80 }}>Qty</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 100 }}>Rate/SFT</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 100 }}>Area (SFT)</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 110 }}>Amount</TableCell>
                <TableCell sx={{ width: 48 }} />
              </TableRow>
            </TableHead>
            <TableBody>
              {fields.map((field, index) => (
                <TableRow key={field.id}>
                  <TableCell>
                    <Controller
                      name={`lineItems.${index}.sectionProfileId`}
                      control={control}
                      render={({ field: f }) => (
                        <Autocomplete
                          size="small"
                          options={spOptions}
                          getOptionLabel={(o) => o.name}
                          value={spOptions.find((s) => s.id === f.value) ?? null}
                          onChange={(_, val) => f.onChange(val?.id ?? 0)}
                          renderInput={(params) => (
                            <TextField
                              {...params}
                              error={!!errors.lineItems?.[index]?.sectionProfileId}
                              placeholder="Select"
                              sx={{ minWidth: 160 }}
                            />
                          )}
                          isOptionEqualToValue={(opt, val) => opt.id === val.id}
                        />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lineItems.${index}.perimeterMM`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField
                          {...f}
                          type="number"
                          size="small"
                          error={!!errors.lineItems?.[index]?.perimeterMM}
                          sx={{ width: 90 }}
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
                          type="number"
                          size="small"
                          error={!!errors.lineItems?.[index]?.lengthMM}
                          sx={{ width: 90 }}
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
                          type="number"
                          size="small"
                          error={!!errors.lineItems?.[index]?.quantity}
                          sx={{ width: 70 }}
                        />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lineItems.${index}.ratePerSFT`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField
                          {...f}
                          type="number"
                          size="small"
                          error={!!errors.lineItems?.[index]?.ratePerSFT}
                          sx={{ width: 90 }}
                        />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {lineTotals[index]?.area.toFixed(2) ?? '0.00'}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {formatCurrency(lineTotals[index]?.amount ?? 0)}
                    </Typography>
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
            <TableFooter>
              <TableRow>
                <TableCell colSpan={5} sx={{ fontWeight: 600, textAlign: 'right' }}>
                  Totals
                </TableCell>
                <TableCell sx={{ fontWeight: 600 }}>{totalSFT.toFixed(2)}</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>{formatCurrency(subTotal)}</TableCell>
                <TableCell />
              </TableRow>
            </TableFooter>
          </Table>
        </TableContainer>

        {/* Summary Card */}
        <Card variant="outlined" sx={{ maxWidth: 400, ml: 'auto', mb: 3 }}>
          <CardContent>
            <Typography variant="subtitle2" fontWeight={600} gutterBottom>
              Invoice Summary
            </Typography>
            <SummaryRow label="Sub Total" value={formatCurrency(subTotal)} />
            {addCharges > 0 && <SummaryRow label="Additional Charges" value={formatCurrency(addCharges)} />}
            <SummaryRow label="Taxable Amount" value={formatCurrency(taxableAmount)} />
            {!isInterState ? (
              <>
                <SummaryRow label={`CGST (${cgstRate}%)`} value={formatCurrency(cgstAmount)} />
                <SummaryRow label={`SGST (${sgstRate}%)`} value={formatCurrency(sgstAmount)} />
              </>
            ) : (
              <SummaryRow label={`IGST (${igstRate}%)`} value={formatCurrency(igstAmount)} />
            )}
            {roundOff !== 0 && (
              <SummaryRow label="Round Off" value={`₹${roundOff >= 0 ? '+' : ''}${roundOff.toFixed(2)}`} />
            )}
            <Divider sx={{ my: 1 }} />
            <SummaryRow label="Grand Total" value={formatCurrency(grandTotal)} bold />
          </CardContent>
        </Card>

        {/* Buttons */}
        <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
          <Button variant="outlined" onClick={() => navigate('/dispatch/invoices')}>
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
              'Update Invoice'
            ) : (
              'Save Invoice'
            )}
          </Button>
        </Box>
      </Box>
    </>
  );
}

function SummaryRow({ label, value, bold }) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', py: 0.25 }}>
      <Typography variant="body2" fontWeight={bold ? 600 : 400}>
        {label}
      </Typography>
      <Typography variant="body2" fontWeight={bold ? 600 : 400}>
        {value}
      </Typography>
    </Box>
  );
}
