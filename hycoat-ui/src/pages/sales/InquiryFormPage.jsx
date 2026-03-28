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
  MenuItem,
  Autocomplete,
  Chip,
} from '@mui/material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import { useInquiry, useCreateInquiry, useUpdateInquiry } from '@/hooks/useInquiries';
import { useCustomerLookup } from '@/hooks/useCustomers';
import { useProcessTypes } from '@/hooks/useProcessTypes';

const SOURCES = ['Email', 'Phone', 'WhatsApp', 'Walk-in', 'Tender'];

const inquirySchema = z.object({
  customerId: z.number({ required_error: 'Customer is required' }).min(1, 'Customer is required'),
  date: z.string().min(1, 'Date is required'),
  source: z.string().min(1, 'Source is required'),
  projectName: z.string().max(300).optional().or(z.literal('')),
  processTypeId: z.number().nullable().optional(),
  contactPerson: z.string().max(200).optional().or(z.literal('')),
  contactEmail: z
    .string()
    .optional()
    .or(z.literal(''))
    .refine((v) => !v || z.string().email().safeParse(v).success, 'Invalid email'),
  contactPhone: z.string().max(20).optional().or(z.literal('')),
  description: z.string().max(2000).optional().or(z.literal('')),
  assignedToUserId: z.string().optional().or(z.literal('')),
});

const defaultValues = {
  customerId: 0,
  date: new Date().toISOString().split('T')[0],
  source: '',
  projectName: '',
  processTypeId: null,
  contactPerson: '',
  contactEmail: '',
  contactPhone: '',
  description: '',
  assignedToUserId: '',
};

const STATUS_COLORS = {
  New: 'info',
  QuotationSent: 'primary',
  BOMReceived: 'secondary',
  PISent: 'warning',
  Confirmed: 'success',
  Lost: 'error',
  Closed: 'default',
};

export default function InquiryFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingInquiry } = useInquiry(id);
  const createMutation = useCreateInquiry();
  const updateMutation = useUpdateInquiry();
  const { data: customers } = useCustomerLookup();
  const { data: processTypes } = useProcessTypes();

  const customerOptions = customers?.data ?? [];
  const processTypeOptions = processTypes?.data ?? [];

  const {
    control,
    handleSubmit,
    reset,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(inquirySchema),
    defaultValues,
  });

  const selectedCustomerId = watch('customerId');
  const existingData = existing?.data;

  useEffect(() => {
    if (existingData) {
      reset({
        customerId: existingData.customerId ?? 0,
        date: existingData.date ? existingData.date.split('T')[0] : '',
        source: existingData.source ?? '',
        projectName: existingData.projectName ?? '',
        processTypeId: existingData.processTypeId ?? null,
        contactPerson: existingData.contactPerson ?? '',
        contactEmail: existingData.contactEmail ?? '',
        contactPhone: existingData.contactPhone ?? '',
        description: existingData.description ?? '',
        assignedToUserId: existingData.assignedToUserId ?? '',
      });
    }
  }, [existingData, reset]);

  const onSubmit = (data) => {
    // Clean up null/empty values
    const payload = {
      ...data,
      processTypeId: data.processTypeId || null,
      assignedToUserId: data.assignedToUserId || null,
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    mutation.mutate(mutationPayload, {
      onSuccess: () => {
        toast.success(`Inquiry ${isEdit ? 'updated' : 'created'}`);
        navigate('/sales/inquiries');
      },
      onError: (err) => {
        const msg =
          err.response?.data?.errors?.[0] ||
          err.response?.data?.message ||
          'Failed to save inquiry';
        toast.error(msg);
      },
    });
  };

  if (isEdit && loadingInquiry) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <>
      <PageHeader
        title={isEdit ? `Edit ${existingData?.inquiryNumber ?? 'Inquiry'}` : 'New Inquiry'}
        subtitle={
          isEdit && existingData?.status ? undefined : undefined
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

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ maxWidth: 800 }}>
        <Grid container spacing={2}>
          {/* Customer Autocomplete */}
          <Grid size={{ xs: 12, sm: 8 }}>
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

          {/* Source */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="source"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  select
                  label="Source"
                  required
                  fullWidth
                  size="small"
                  error={!!errors.source}
                  helperText={errors.source?.message}
                >
                  {SOURCES.map((s) => (
                    <MenuItem key={s} value={s}>
                      {s}
                    </MenuItem>
                  ))}
                </TextField>
              )}
            />
          </Grid>

          {/* Project Name */}
          <Grid size={{ xs: 12, sm: 8 }}>
            <Field control={control} name="projectName" label="Project Name" errors={errors} />
          </Grid>

          {/* Process Type */}
          <Grid size={{ xs: 12, sm: 6 }}>
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
                  renderInput={(params) => (
                    <TextField {...params} label="Process Type" />
                  )}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          <Grid size={12}>
            <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 1, mt: 1 }} />
          </Grid>

          {/* Contact Details */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="contactPerson" label="Contact Person" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="contactEmail" label="Contact Email" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="contactPhone" label="Contact Phone" errors={errors} />
          </Grid>

          {/* Description */}
          <Grid size={12}>
            <Field
              control={control}
              name="description"
              label="Description"
              multiline
              rows={3}
              errors={errors}
            />
          </Grid>

          {/* Buttons */}
          <Grid size={12}>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 1 }}>
              <Button variant="outlined" onClick={() => navigate('/sales/inquiries')}>
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
                  'Update Inquiry'
                ) : (
                  'Save Inquiry'
                )}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Box>
    </>
  );
}

function Field({ control, name, label, errors, required, ...props }) {
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
        />
      )}
    />
  );
}
