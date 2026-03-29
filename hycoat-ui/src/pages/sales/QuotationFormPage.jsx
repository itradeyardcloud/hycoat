import { useEffect } from 'react';
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
import { useQuotation, useCreateQuotation, useUpdateQuotation } from '@/hooks/useQuotations';
import { useCustomerLookup } from '@/hooks/useCustomers';
import { useProcessTypes } from '@/hooks/useProcessTypes';
import { useInquiryLookup } from '@/hooks/useInquiries';

const lineSchema = z.object({
  processTypeId: z.number({ required_error: 'Required' }).min(1, 'Required'),
  description: z.string().max(1000).optional().or(z.literal('')),
  ratePerSFT: z.coerce.number().min(0.01, 'Rate must be positive'),
  warrantyYears: z.coerce.number().nullable().optional(),
  micronRange: z.string().max(100).optional().or(z.literal('')),
});

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  inquiryId: z.number().nullable().optional(),
  customerId: z.number({ required_error: 'Customer is required' }).min(1, 'Customer is required'),
  validityDays: z.coerce.number().min(1, 'Min 1 day').max(365, 'Max 365 days'),
  notes: z.string().max(2000).optional().or(z.literal('')),
  lineItems: z.array(lineSchema).min(1, 'At least one line item is required'),
});

const emptyLine = {
  processTypeId: 0,
  description: '',
  ratePerSFT: 0,
  warrantyYears: null,
  micronRange: '',
};

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  inquiryId: null,
  customerId: 0,
  validityDays: 30,
  notes: '',
  lineItems: [{ ...emptyLine }],
};

const STATUS_COLORS = {
  Draft: 'default',
  Sent: 'info',
  Accepted: 'success',
  Rejected: 'error',
  Expired: 'warning',
};

export default function QuotationFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingExisting } = useQuotation(id);
  const createMutation = useCreateQuotation();
  const updateMutation = useUpdateQuotation();
  const { data: customers } = useCustomerLookup();
  const { data: inquiries } = useInquiryLookup();
  const { data: processTypes } = useProcessTypes();

  const customerOptions = customers?.data ?? [];
  const inquiryOptions = inquiries?.data ?? [];
  const processTypeOptions = processTypes?.data ?? [];

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
  const existingData = existing?.data;

  useEffect(() => {
    if (existingData) {
      reset({
        date: existingData.date ? existingData.date.split('T')[0] : '',
        inquiryId: existingData.inquiryId ?? null,
        customerId: existingData.customerId ?? 0,
        validityDays: existingData.validityDays ?? 30,
        notes: existingData.notes ?? '',
        lineItems: existingData.lineItems?.length
          ? existingData.lineItems.map((l) => ({
              processTypeId: l.processTypeId ?? 0,
              description: l.description ?? '',
              ratePerSFT: l.ratePerSFT ?? 0,
              warrantyYears: l.warrantyYears ?? null,
              micronRange: l.micronRange ?? '',
            }))
          : [{ ...emptyLine }],
      });
    }
  }, [existingData, reset]);

  const onSubmit = (data) => {
    const payload = {
      ...data,
      inquiryId: data.inquiryId || null,
      notes: data.notes || null,
      lineItems: data.lineItems.map((line) => ({
        ...line,
        description: line.description || null,
        micronRange: line.micronRange || null,
        warrantyYears: line.warrantyYears || null,
      })),
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    mutation.mutate(mutationPayload, {
      onSuccess: () => {
        toast.success(`Quotation ${isEdit ? 'updated' : 'created'}`);
        navigate('/sales/quotations');
      },
      onError: (err) => {
        const msg = err.response?.data?.errors?.[0] || err.response?.data?.message || 'Failed to save quotation';
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
        title={isEdit ? `Edit ${existingData?.quotationNumber ?? 'Quotation'}` : 'New Quotation'}
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
              name="inquiryId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={inquiryOptions}
                  getOptionLabel={(o) => o.name}
                  value={inquiryOptions.find((inq) => inq.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? null)}
                  renderInput={(params) => <TextField {...params} label="Inquiry (Optional)" />}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="validityDays"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Validity (Days)"
                  type="number"
                  required
                  fullWidth
                  size="small"
                  error={!!errors.validityDays}
                  helperText={errors.validityDays?.message}
                />
              )}
            />
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
                <TableCell sx={{ width: 60 }}>#</TableCell>
                <TableCell>Process Type</TableCell>
                <TableCell>Description</TableCell>
                <TableCell sx={{ width: 140 }}>Rate / SFT</TableCell>
                <TableCell sx={{ width: 140 }}>Warranty (Years)</TableCell>
                <TableCell sx={{ width: 160 }}>Micron Range</TableCell>
                <TableCell sx={{ width: 60 }} />
              </TableRow>
            </TableHead>
            <TableBody>
              {fields.map((field, index) => (
                <TableRow key={field.id}>
                  <TableCell>{index + 1}</TableCell>
                  <TableCell>
                    <Controller
                      name={`lineItems.${index}.processTypeId`}
                      control={control}
                      render={({ field: f }) => (
                        <Autocomplete
                          size="small"
                          options={processTypeOptions}
                          getOptionLabel={(o) => o.name}
                          value={processTypeOptions.find((pt) => pt.id === f.value) ?? null}
                          onChange={(_, val) => f.onChange(val?.id ?? 0)}
                          renderInput={(params) => (
                            <TextField
                              {...params}
                              error={!!errors.lineItems?.[index]?.processTypeId}
                              helperText={errors.lineItems?.[index]?.processTypeId?.message}
                            />
                          )}
                          isOptionEqualToValue={(opt, val) => opt.id === val.id}
                        />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lineItems.${index}.description`}
                      control={control}
                      render={({ field: f }) => <TextField {...f} size="small" fullWidth />}
                    />
                  </TableCell>
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
                  <TableCell>
                    <Controller
                      name={`lineItems.${index}.warrantyYears`}
                      control={control}
                      render={({ field: f }) => <TextField {...f} size="small" type="number" fullWidth />}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lineItems.${index}.micronRange`}
                      control={control}
                      render={({ field: f }) => <TextField {...f} size="small" fullWidth />}
                    />
                  </TableCell>
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

        <Box sx={{ mb: 3 }}>
          <Button startIcon={<Add />} onClick={() => append({ ...emptyLine })}>
            Add Line Item
          </Button>
        </Box>

        <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
          <Button variant="outlined" onClick={() => navigate('/sales/quotations')}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={isSubmitting || createMutation.isPending || updateMutation.isPending}>
            {isSubmitting || createMutation.isPending || updateMutation.isPending ? (
              <CircularProgress size={22} />
            ) : isEdit ? (
              'Update Quotation'
            ) : (
              'Save Quotation'
            )}
          </Button>
        </Box>
      </Box>
    </>
  );
}
