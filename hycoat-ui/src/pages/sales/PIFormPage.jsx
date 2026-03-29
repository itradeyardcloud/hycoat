import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
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
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Chip,
  FormControlLabel,
  Switch,
  Typography,
} from '@mui/material';
import { Add, Delete, Calculate } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  useProformaInvoice,
  useCreatePI,
  useUpdatePI,
  useCalculateArea,
} from '@/hooks/useProformaInvoices';
import { useCustomerLookup } from '@/hooks/useCustomers';
import { useQuotationLookup } from '@/hooks/useQuotations';
import { useSectionProfileLookup } from '@/hooks/useSectionProfiles';
import { formatCurrency, formatNumber } from '@/utils/formatters';

const lineSchema = z.object({
  sectionProfileId: z.number({ required_error: 'Required' }).min(1, 'Required'),
  lengthMM: z.coerce.number().min(1, 'Min 1'),
  quantity: z.coerce.number().int().min(1, 'Min 1'),
  ratePerSFT: z.coerce.number().min(0.01, 'Min 0.01'),
});

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  customerId: z.number({ required_error: 'Customer is required' }).min(1, 'Customer is required'),
  quotationId: z.number().nullable().optional(),
  packingCharges: z.coerce.number().min(0, 'Cannot be negative'),
  transportCharges: z.coerce.number().min(0, 'Cannot be negative'),
  isInterState: z.boolean(),
  notes: z.string().max(2000).optional().or(z.literal('')),
  lineItems: z.array(lineSchema).min(1, 'At least one line item is required'),
});

const emptyLine = {
  sectionProfileId: 0,
  lengthMM: 0,
  quantity: 1,
  ratePerSFT: 0,
};

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  customerId: 0,
  quotationId: null,
  packingCharges: 0,
  transportCharges: 0,
  isInterState: false,
  notes: '',
  lineItems: [{ ...emptyLine }],
};

const STATUS_COLORS = {
  Draft: 'default',
  Sent: 'info',
  Accepted: 'success',
  Rejected: 'error',
};

export default function PIFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingExisting } = useProformaInvoice(id);
  const createMutation = useCreatePI();
  const updateMutation = useUpdatePI();
  const calculateAreaMutation = useCalculateArea();

  const { data: customers } = useCustomerLookup();
  const { data: quotations } = useQuotationLookup();
  const { data: sectionProfiles } = useSectionProfileLookup();

  const customerOptions = customers?.data ?? [];
  const quotationOptions = quotations?.data ?? [];
  const sectionProfileOptions = sectionProfiles?.data ?? [];

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues,
  });

  const { fields, append, remove } = useFieldArray({ control, name: 'lineItems' });
  const watchedLines = useWatch({ control, name: 'lineItems' }) || [];
  const packingCharges = Number(useWatch({ control, name: 'packingCharges' }) || 0);
  const transportCharges = Number(useWatch({ control, name: 'transportCharges' }) || 0);
  const isInterState = !!useWatch({ control, name: 'isInterState' });

  const [areasByIndex, setAreasByIndex] = useState({});
  const existingData = existing?.data;

  useEffect(() => {
    if (existingData) {
      const lines = existingData.lineItems?.length
        ? existingData.lineItems.map((l) => ({
            sectionProfileId: l.sectionProfileId ?? 0,
            lengthMM: l.lengthMM ?? 0,
            quantity: l.quantity ?? 1,
            ratePerSFT: l.ratePerSFT ?? 0,
          }))
        : [{ ...emptyLine }];

      reset({
        date: existingData.date ? existingData.date.split('T')[0] : '',
        customerId: existingData.customerId ?? 0,
        quotationId: existingData.quotationId ?? null,
        packingCharges: existingData.packingCharges ?? 0,
        transportCharges: existingData.transportCharges ?? 0,
        isInterState: !!existingData.isInterState,
        notes: existingData.notes ?? '',
        lineItems: lines,
      });

      const areaMap = {};
      (existingData.lineItems || []).forEach((line, index) => {
        areaMap[index] = Number(line.areaSFT || 0);
      });
      setAreasByIndex(areaMap);
    }
  }, [existingData, reset]);

  const lineAmounts = useMemo(
    () => watchedLines.map((line, index) => (Number(areasByIndex[index] || 0) * Number(line.ratePerSFT || 0))),
    [areasByIndex, watchedLines],
  );

  const subTotal = lineAmounts.reduce((sum, amt) => sum + amt, 0);
  const taxableAmount = subTotal + packingCharges + transportCharges;
  const cgstAmount = isInterState ? 0 : taxableAmount * 0.09;
  const sgstAmount = isInterState ? 0 : taxableAmount * 0.09;
  const igstAmount = isInterState ? taxableAmount * 0.18 : 0;
  const grandTotal = taxableAmount + cgstAmount + sgstAmount + igstAmount;

  const handleCalculateArea = async () => {
    try {
      const payload = {
        lines: watchedLines.map((line) => ({
          sectionProfileId: Number(line.sectionProfileId || 0),
          lengthMM: Number(line.lengthMM || 0),
          quantity: Number(line.quantity || 0),
        })),
      };

      const response = await calculateAreaMutation.mutateAsync(payload);
      const areaMap = {};
      (response?.data?.lines || []).forEach((line, index) => {
        areaMap[index] = Number(line.areaSFT || 0);
      });
      setAreasByIndex(areaMap);
      toast.success('Area calculated');
    } catch (err) {
      const msg = err.response?.data?.errors?.[0] || err.response?.data?.message || 'Failed to calculate area';
      toast.error(msg);
    }
  };

  const onSubmit = (data) => {
    const payload = {
      ...data,
      quotationId: data.quotationId || null,
      notes: data.notes || null,
      lineItems: data.lineItems.map((line) => ({
        sectionProfileId: Number(line.sectionProfileId),
        lengthMM: Number(line.lengthMM),
        quantity: Number(line.quantity),
        ratePerSFT: Number(line.ratePerSFT),
      })),
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    mutation.mutate(mutationPayload, {
      onSuccess: () => {
        toast.success(`Proforma invoice ${isEdit ? 'updated' : 'created'}`);
        navigate('/sales/proforma-invoices');
      },
      onError: (err) => {
        const msg = err.response?.data?.errors?.[0] || err.response?.data?.message || 'Failed to save PI';
        toast.error(msg);
      },
    });
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
      <PageHeader
        title={isEdit ? `Edit ${existingData?.piNumber || existingData?.pINumber || 'PI'}` : 'New Proforma Invoice'}
        action={
          isEdit && existingData?.status ? (
            <Chip label={existingData.status} color={STATUS_COLORS[existingData.status] || 'default'} />
          ) : undefined
        }
      />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <Grid container spacing={2} sx={{ maxWidth: 1000, mb: 3 }}>
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

          <Grid size={{ xs: 12, sm: 5 }}>
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

          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="quotationId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={quotationOptions}
                  getOptionLabel={(o) => o.name}
                  value={quotationOptions.find((q) => q.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? null)}
                  renderInput={(params) => <TextField {...params} label="Quotation (Optional)" />}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          <Grid size={{ xs: 12, sm: 3 }}>
            <Controller
              name="packingCharges"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Packing Charges"
                  type="number"
                  fullWidth
                  size="small"
                  error={!!errors.packingCharges}
                  helperText={errors.packingCharges?.message}
                />
              )}
            />
          </Grid>

          <Grid size={{ xs: 12, sm: 3 }}>
            <Controller
              name="transportCharges"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Transport Charges"
                  type="number"
                  fullWidth
                  size="small"
                  error={!!errors.transportCharges}
                  helperText={errors.transportCharges?.message}
                />
              )}
            />
          </Grid>

          <Grid size={{ xs: 12, sm: 3 }} sx={{ display: 'flex', alignItems: 'center' }}>
            <Controller
              name="isInterState"
              control={control}
              render={({ field }) => (
                <FormControlLabel control={<Switch checked={!!field.value} onChange={(e) => field.onChange(e.target.checked)} />} label="Inter-State (IGST)" />
              )}
            />
          </Grid>

          <Grid size={{ xs: 12, sm: 3 }} sx={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end' }}>
            <Button variant="outlined" startIcon={<Calculate />} onClick={handleCalculateArea} disabled={calculateAreaMutation.isPending}>
              Calculate Area
            </Button>
          </Grid>

          <Grid size={12}>
            <Controller
              name="notes"
              control={control}
              render={({ field }) => (
                <TextField {...field} label="Notes" fullWidth size="small" multiline rows={2} />
              )}
            />
          </Grid>
        </Grid>

        <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell sx={{ width: 50 }}>#</TableCell>
                <TableCell>Section Profile</TableCell>
                <TableCell sx={{ width: 140 }}>Length (mm)</TableCell>
                <TableCell sx={{ width: 110 }}>Qty</TableCell>
                <TableCell sx={{ width: 140 }}>Area (SFT)</TableCell>
                <TableCell sx={{ width: 140 }}>Rate / SFT</TableCell>
                <TableCell sx={{ width: 160 }}>Amount</TableCell>
                <TableCell sx={{ width: 60 }} />
              </TableRow>
            </TableHead>
            <TableBody>
              {fields.map((field, index) => (
                <TableRow key={field.id}>
                  <TableCell>{index + 1}</TableCell>
                  <TableCell>
                    <Controller
                      name={`lineItems.${index}.sectionProfileId`}
                      control={control}
                      render={({ field: f }) => (
                        <Autocomplete
                          size="small"
                          options={sectionProfileOptions}
                          getOptionLabel={(o) => o.name}
                          value={sectionProfileOptions.find((sp) => sp.id === f.value) ?? null}
                          onChange={(_, val) => f.onChange(val?.id ?? 0)}
                          renderInput={(params) => (
                            <TextField
                              {...params}
                              error={!!errors.lineItems?.[index]?.sectionProfileId}
                              helperText={errors.lineItems?.[index]?.sectionProfileId?.message}
                            />
                          )}
                          isOptionEqualToValue={(opt, val) => opt.id === val.id}
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
                          fullWidth
                          error={!!errors.lineItems?.[index]?.lengthMM}
                          helperText={errors.lineItems?.[index]?.lengthMM?.message}
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
                          fullWidth
                          error={!!errors.lineItems?.[index]?.quantity}
                          helperText={errors.lineItems?.[index]?.quantity?.message}
                        />
                      )}
                    />
                  </TableCell>
                  <TableCell>{formatNumber(areasByIndex[index] || 0)}</TableCell>
                  <TableCell>
                    <Controller
                      name={`lineItems.${index}.ratePerSFT`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField
                          {...f}
                          size="small"
                          type="number"
                          fullWidth
                          error={!!errors.lineItems?.[index]?.ratePerSFT}
                          helperText={errors.lineItems?.[index]?.ratePerSFT?.message}
                        />
                      )}
                    />
                  </TableCell>
                  <TableCell>{formatCurrency(lineAmounts[index] || 0)}</TableCell>
                  <TableCell>
                    <IconButton size="small" color="error" onClick={() => remove(index)} disabled={fields.length === 1}>
                      <Delete fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        <Box sx={{ mb: 2 }}>
          <Button startIcon={<Add />} onClick={() => append({ ...emptyLine })}>
            Add Line Item
          </Button>
        </Box>

        <Box sx={{ maxWidth: 360, ml: 'auto', mb: 3 }}>
          <Typography variant="body2">Sub Total: {formatCurrency(subTotal)}</Typography>
          <Typography variant="body2">Packing: {formatCurrency(packingCharges)}</Typography>
          <Typography variant="body2">Transport: {formatCurrency(transportCharges)}</Typography>
          <Typography variant="body2">Taxable: {formatCurrency(taxableAmount)}</Typography>
          {isInterState ? (
            <Typography variant="body2">IGST (18%): {formatCurrency(igstAmount)}</Typography>
          ) : (
            <>
              <Typography variant="body2">CGST (9%): {formatCurrency(cgstAmount)}</Typography>
              <Typography variant="body2">SGST (9%): {formatCurrency(sgstAmount)}</Typography>
            </>
          )}
          <Typography variant="h6" sx={{ mt: 0.5 }}>
            Grand Total: {formatCurrency(grandTotal)}
          </Typography>
        </Box>

        <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
          <Button variant="outlined" onClick={() => navigate('/sales/proforma-invoices')}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={isSubmitting || createMutation.isPending || updateMutation.isPending}>
            {isSubmitting || createMutation.isPending || updateMutation.isPending ? (
              <CircularProgress size={22} />
            ) : isEdit ? (
              'Update PI'
            ) : (
              'Save PI'
            )}
          </Button>
        </Box>
      </Box>
    </>
  );
}
