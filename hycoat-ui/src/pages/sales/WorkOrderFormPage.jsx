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
  Autocomplete,
  Chip,
} from '@mui/material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import { useWorkOrder, useCreateWorkOrder, useUpdateWorkOrder } from '@/hooks/useWorkOrders';
import { useCustomerLookup } from '@/hooks/useCustomers';
import { usePILookup } from '@/hooks/useProformaInvoices';
import { useProcessTypes } from '@/hooks/useProcessTypes';
import { usePowderColorLookup } from '@/hooks/usePowderColors';

const schema = z
  .object({
    date: z.string().min(1, 'Date is required'),
    customerId: z.number({ required_error: 'Customer is required' }).min(1, 'Customer is required'),
    proformaInvoiceId: z.number().nullable().optional(),
    projectName: z.string().max(300).optional().or(z.literal('')),
    processTypeId: z.number({ required_error: 'Process is required' }).min(1, 'Process is required'),
    powderColorId: z.number().nullable().optional(),
    dispatchDate: z.string().optional().or(z.literal('')),
    notes: z.string().max(2000).optional().or(z.literal('')),
  })
  .refine((data) => !data.dispatchDate || !data.date || new Date(data.dispatchDate) >= new Date(data.date), {
    message: 'Dispatch date cannot be before date',
    path: ['dispatchDate'],
  });

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  customerId: 0,
  proformaInvoiceId: null,
  projectName: '',
  processTypeId: 0,
  powderColorId: null,
  dispatchDate: '',
  notes: '',
};

const STATUS_COLORS = {
  Created: 'default',
  MaterialAwaited: 'warning',
  MaterialReceived: 'info',
  InProduction: 'primary',
  QAComplete: 'secondary',
  Dispatched: 'success',
  Invoiced: 'success',
  Closed: 'default',
};

export default function WorkOrderFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingExisting } = useWorkOrder(id);
  const createMutation = useCreateWorkOrder();
  const updateMutation = useUpdateWorkOrder();

  const { data: customers } = useCustomerLookup();
  const { data: piLookup } = usePILookup();
  const { data: processTypes } = useProcessTypes();
  const { data: powderColors } = usePowderColorLookup();

  const customerOptions = customers?.data ?? [];
  const piOptions = piLookup?.data ?? [];
  const processTypeOptions = processTypes?.data ?? [];
  const powderColorOptions = powderColors?.data ?? [];

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues,
  });

  const existingData = existing?.data;

  useEffect(() => {
    if (existingData) {
      reset({
        date: existingData.date ? existingData.date.split('T')[0] : '',
        customerId: existingData.customerId ?? 0,
        proformaInvoiceId: existingData.proformaInvoiceId ?? null,
        projectName: existingData.projectName ?? '',
        processTypeId: existingData.processTypeId ?? 0,
        powderColorId: existingData.powderColorId ?? null,
        dispatchDate: existingData.dispatchDate ? existingData.dispatchDate.split('T')[0] : '',
        notes: existingData.notes ?? '',
      });
    }
  }, [existingData, reset]);

  const onSubmit = (data) => {
    const payload = {
      ...data,
      proformaInvoiceId: data.proformaInvoiceId || null,
      projectName: data.projectName || null,
      powderColorId: data.powderColorId || null,
      dispatchDate: data.dispatchDate || null,
      notes: data.notes || null,
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    mutation.mutate(mutationPayload, {
      onSuccess: (res) => {
        const newId = res?.data?.id;
        toast.success(`Work order ${isEdit ? 'updated' : 'created'}`);
        navigate(isEdit ? `/sales/work-orders/${id}` : (newId ? `/sales/work-orders/${newId}` : '/sales/work-orders'));
      },
      onError: (err) => {
        const msg = err.response?.data?.errors?.[0] || err.response?.data?.message || 'Failed to save work order';
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
        title={isEdit ? `Edit ${existingData?.woNumber || existingData?.wONumber || 'Work Order'}` : 'New Work Order'}
        action={
          isEdit && existingData?.status ? (
            <Chip label={existingData.status} color={STATUS_COLORS[existingData.status] || 'default'} />
          ) : undefined
        }
      />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ maxWidth: 900 }}>
        <Grid container spacing={2}>
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
              name="proformaInvoiceId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={piOptions}
                  getOptionLabel={(o) => o.name}
                  value={piOptions.find((pi) => pi.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? null)}
                  renderInput={(params) => <TextField {...params} label="PI (Optional)" />}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          <Grid size={{ xs: 12, sm: 6 }}>
            <Controller
              name="projectName"
              control={control}
              render={({ field }) => <TextField {...field} label="Project Name" fullWidth size="small" />}
            />
          </Grid>

          <Grid size={{ xs: 12, sm: 3 }}>
            <Controller
              name="processTypeId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={processTypeOptions}
                  getOptionLabel={(o) => o.name}
                  value={processTypeOptions.find((pt) => pt.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? 0)}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Process Type"
                      required
                      error={!!errors.processTypeId}
                      helperText={errors.processTypeId?.message}
                    />
                  )}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          <Grid size={{ xs: 12, sm: 3 }}>
            <Controller
              name="powderColorId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={powderColorOptions}
                  getOptionLabel={(o) => o.name}
                  value={powderColorOptions.find((pc) => pc.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? null)}
                  renderInput={(params) => <TextField {...params} label="Powder Color (Optional)" />}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="dispatchDate"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Dispatch Date"
                  type="date"
                  fullWidth
                  size="small"
                  error={!!errors.dispatchDate}
                  helperText={errors.dispatchDate?.message}
                  slotProps={{ inputLabel: { shrink: true } }}
                />
              )}
            />
          </Grid>

          <Grid size={12}>
            <Controller
              name="notes"
              control={control}
              render={({ field }) => <TextField {...field} label="Notes" fullWidth size="small" multiline rows={3} />}
            />
          </Grid>

          <Grid size={12}>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 1 }}>
              <Button variant="outlined" onClick={() => navigate('/sales/work-orders')}>
                Cancel
              </Button>
              <Button type="submit" variant="contained" disabled={isSubmitting || createMutation.isPending || updateMutation.isPending}>
                {isSubmitting || createMutation.isPending || updateMutation.isPending ? (
                  <CircularProgress size={22} />
                ) : isEdit ? (
                  'Update Work Order'
                ) : (
                  'Save Work Order'
                )}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Box>
    </>
  );
}
