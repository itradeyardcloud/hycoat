import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Box, Button, TextField, Grid, CircularProgress, Autocomplete } from '@mui/material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import { usePowderColor, useCreatePowderColor, useUpdatePowderColor } from '@/hooks/usePowderColors';
import { useVendorLookup } from '@/hooks/useVendors';

const schema = z.object({
  powderCode: z.string().min(1, 'Powder code is required').max(50),
  colorName: z.string().min(1, 'Color name is required').max(200),
  ralCode: z.string().max(50).optional().or(z.literal('')),
  make: z.string().max(200).optional().or(z.literal('')),
  vendorId: z.coerce.number().positive().nullable().optional(),
  warrantyYears: z.coerce.number().int().min(0).optional().or(z.literal('')),
  notes: z.string().max(1000).optional().or(z.literal('')),
});

const defaultValues = {
  powderCode: '',
  colorName: '',
  ralCode: '',
  make: '',
  vendorId: null,
  warrantyYears: '',
  notes: '',
};

export default function PowderColorFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingColor } = usePowderColor(id);
  const createMutation = useCreatePowderColor();
  const updateMutation = useUpdatePowderColor();
  const { data: vendorsResponse } = useVendorLookup();
  const vendors = vendorsResponse?.data ?? [];

  const {
    control,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues,
  });

  const vendorIdValue = watch('vendorId');

  useEffect(() => {
    if (existing?.data) {
      reset({
        powderCode: existing.data.powderCode ?? '',
        colorName: existing.data.colorName ?? '',
        ralCode: existing.data.ralCode ?? '',
        make: existing.data.make ?? '',
        vendorId: existing.data.vendorId ?? null,
        warrantyYears: existing.data.warrantyYears ?? '',
        notes: existing.data.notes ?? '',
      });
    }
  }, [existing, reset]);

  const onSubmit = (data) => {
    const payload = {
      ...data,
      warrantyYears: data.warrantyYears || null,
      vendorId: data.vendorId || null,
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    mutation.mutate(mutationPayload, {
      onSuccess: () => {
        toast.success(`Powder color ${isEdit ? 'updated' : 'created'}`);
        navigate('/masters/powder-colors');
      },
      onError: (err) => {
        const msg = err.response?.data?.errors?.[0] || err.response?.data?.message || 'Failed to save powder color';
        toast.error(msg);
      },
    });
  };

  if (isEdit && loadingColor) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <>
      <PageHeader title={isEdit ? 'Edit Powder Color' : 'Add Powder Color'} />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ maxWidth: 800 }}>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Field control={control} name="powderCode" label="Powder Code" required errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Field control={control} name="colorName" label="Color Name" required errors={errors} />
          </Grid>

          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="ralCode" label="RAL Code" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="make" label="Make" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="warrantyYears" label="Warranty (years)" type="number" errors={errors} />
          </Grid>

          <Grid size={{ xs: 12, sm: 6 }}>
            <Autocomplete
              options={vendors}
              getOptionLabel={(o) => o.name ?? ''}
              value={vendors.find((v) => v.id === vendorIdValue) ?? null}
              onChange={(_, newVal) => setValue('vendorId', newVal?.id ?? null, { shouldValidate: true })}
              renderInput={(params) => (
                <TextField {...params} label="Vendor" size="small" error={!!errors.vendorId} helperText={errors.vendorId?.message} />
              )}
              isOptionEqualToValue={(opt, val) => opt.id === val.id}
            />
          </Grid>

          <Grid size={12}>
            <Field control={control} name="notes" label="Notes" multiline rows={3} errors={errors} />
          </Grid>

          <Grid size={12}>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 1 }}>
              <Button variant="outlined" onClick={() => navigate('/masters/powder-colors')}>
                Cancel
              </Button>
              <Button type="submit" variant="contained" disabled={isSubmitting || createMutation.isPending || updateMutation.isPending}>
                {isSubmitting || createMutation.isPending || updateMutation.isPending ? (
                  <CircularProgress size={22} />
                ) : (
                  'Save Powder Color'
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
