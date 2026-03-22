import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Box, Button, TextField, Grid, CircularProgress, FormControl, InputLabel, Select, MenuItem, FormHelperText } from '@mui/material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import { useVendor, useCreateVendor, useUpdateVendor } from '@/hooks/useVendors';

const GSTIN_REGEX = /^\d{2}[A-Z]{5}\d{4}[A-Z][A-Z\d]Z[A-Z\d]$/;
const VENDOR_TYPES = ['Powder', 'Chemical', 'Consumable', 'Other'];

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(300),
  vendorType: z.string().min(1, 'Vendor type is required'),
  address: z.string().max(500).optional().or(z.literal('')),
  city: z.string().max(100).optional().or(z.literal('')),
  state: z.string().max(100).optional().or(z.literal('')),
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
});

const defaultValues = {
  name: '',
  vendorType: '',
  address: '',
  city: '',
  state: '',
  gstin: '',
  contactPerson: '',
  phone: '',
  email: '',
};

export default function VendorFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingVendor } = useVendor(id);
  const createMutation = useCreateVendor();
  const updateMutation = useUpdateVendor();

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
      reset({
        name: existing.data.name ?? '',
        vendorType: existing.data.vendorType ?? '',
        address: existing.data.address ?? '',
        city: existing.data.city ?? '',
        state: existing.data.state ?? '',
        gstin: existing.data.gstin ?? '',
        contactPerson: existing.data.contactPerson ?? '',
        phone: existing.data.phone ?? '',
        email: existing.data.email ?? '',
      });
    }
  }, [existing, reset]);

  const onSubmit = (data) => {
    const mutation = isEdit ? updateMutation : createMutation;
    const payload = isEdit ? { id: Number(id), data } : data;

    mutation.mutate(payload, {
      onSuccess: () => {
        toast.success(`Vendor ${isEdit ? 'updated' : 'created'}`);
        navigate('/masters/vendors');
      },
      onError: (err) => {
        const msg = err.response?.data?.errors?.[0] || err.response?.data?.message || 'Failed to save vendor';
        toast.error(msg);
      },
    });
  };

  if (isEdit && loadingVendor) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <>
      <PageHeader title={isEdit ? 'Edit Vendor' : 'Add Vendor'} />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ maxWidth: 800 }}>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 8 }}>
            <Field control={control} name="name" label="Vendor Name" required errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="vendorType"
              control={control}
              render={({ field }) => (
                <FormControl fullWidth size="small" error={!!errors.vendorType} required>
                  <InputLabel>Vendor Type</InputLabel>
                  <Select {...field} label="Vendor Type">
                    {VENDOR_TYPES.map((t) => (
                      <MenuItem key={t} value={t}>
                        {t}
                      </MenuItem>
                    ))}
                  </Select>
                  {errors.vendorType && <FormHelperText>{errors.vendorType.message}</FormHelperText>}
                </FormControl>
              )}
            />
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
            <Field control={control} name="gstin" label="GSTIN" errors={errors} />
          </Grid>

          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="contactPerson" label="Contact Person" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="phone" label="Phone" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="email" label="Email" errors={errors} />
          </Grid>

          <Grid size={12}>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 1 }}>
              <Button variant="outlined" onClick={() => navigate('/masters/vendors')}>
                Cancel
              </Button>
              <Button type="submit" variant="contained" disabled={isSubmitting || createMutation.isPending || updateMutation.isPending}>
                {isSubmitting || createMutation.isPending || updateMutation.isPending ? (
                  <CircularProgress size={22} />
                ) : (
                  'Save Vendor'
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
