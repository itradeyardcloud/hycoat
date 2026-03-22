import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Box, Button, TextField, Grid, CircularProgress } from '@mui/material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import { useCustomer, useCreateCustomer, useUpdateCustomer } from '@/hooks/useCustomers';

const GSTIN_REGEX = /^\d{2}[A-Z]{5}\d{4}[A-Z][A-Z\d]Z[A-Z\d]$/;

const customerSchema = z.object({
  name: z.string().min(1, 'Name is required').max(300),
  shortName: z.string().max(50).optional().or(z.literal('')),
  address: z.string().max(500).optional().or(z.literal('')),
  city: z.string().max(100).optional().or(z.literal('')),
  state: z.string().max(100).optional().or(z.literal('')),
  pincode: z
    .string()
    .optional()
    .or(z.literal(''))
    .refine((v) => !v || /^\d{6}$/.test(v), 'Pincode must be 6 digits'),
  gstin: z
    .string()
    .optional()
    .or(z.literal(''))
    .refine((v) => !v || GSTIN_REGEX.test(v), 'Invalid GSTIN format'),
  contactPerson: z.string().max(200).optional().or(z.literal('')),
  phone: z.string().max(20).optional().or(z.literal('')),
  email: z
    .string()
    .optional()
    .or(z.literal(''))
    .refine((v) => !v || z.string().email().safeParse(v).success, 'Invalid email'),
  notes: z.string().max(1000).optional().or(z.literal('')),
});

const defaultValues = {
  name: '',
  shortName: '',
  address: '',
  city: '',
  state: '',
  pincode: '',
  gstin: '',
  contactPerson: '',
  phone: '',
  email: '',
  notes: '',
};

export default function CustomerFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingCustomer } = useCustomer(id);
  const createMutation = useCreateCustomer();
  const updateMutation = useUpdateCustomer();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(customerSchema),
    defaultValues,
  });

  useEffect(() => {
    if (existing?.data) {
      reset({
        name: existing.data.name ?? '',
        shortName: existing.data.shortName ?? '',
        address: existing.data.address ?? '',
        city: existing.data.city ?? '',
        state: existing.data.state ?? '',
        pincode: existing.data.pincode ?? '',
        gstin: existing.data.gstin ?? '',
        contactPerson: existing.data.contactPerson ?? '',
        phone: existing.data.phone ?? '',
        email: existing.data.email ?? '',
        notes: existing.data.notes ?? '',
      });
    }
  }, [existing, reset]);

  const onSubmit = (data) => {
    const mutation = isEdit ? updateMutation : createMutation;
    const payload = isEdit ? { id: Number(id), data } : data;

    mutation.mutate(payload, {
      onSuccess: () => {
        toast.success(`Customer ${isEdit ? 'updated' : 'created'}`);
        navigate('/masters/customers');
      },
      onError: (err) => {
        const msg = err.response?.data?.errors?.[0] || err.response?.data?.message || 'Failed to save customer';
        toast.error(msg);
      },
    });
  };

  if (isEdit && loadingCustomer) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <>
      <PageHeader title={isEdit ? 'Edit Customer' : 'Add Customer'} />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ maxWidth: 800 }}>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 8 }}>
            <Field control={control} name="name" label="Company Name" required errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="shortName" label="Short Name" errors={errors} />
          </Grid>

          <Grid size={12}>
            <Field control={control} name="address" label="Address" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="city" label="City" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="state" label="State" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="pincode" label="Pincode" errors={errors} />
          </Grid>

          <Grid size={{ xs: 12, sm: 6 }}>
            <Field control={control} name="gstin" label="GSTIN" errors={errors} />
          </Grid>

          <Grid size={{ xs: 12, sm: 6 }}>
            <Field control={control} name="contactPerson" label="Contact Person" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Field control={control} name="phone" label="Phone" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Field control={control} name="email" label="Email" errors={errors} />
          </Grid>

          <Grid size={12}>
            <Field control={control} name="notes" label="Notes" multiline rows={3} errors={errors} />
          </Grid>

          <Grid size={12}>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 1 }}>
              <Button variant="outlined" onClick={() => navigate('/masters/customers')}>
                Cancel
              </Button>
              <Button type="submit" variant="contained" disabled={isSubmitting || createMutation.isPending || updateMutation.isPending}>
                {isSubmitting || createMutation.isPending || updateMutation.isPending ? (
                  <CircularProgress size={22} />
                ) : (
                  'Save Customer'
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
