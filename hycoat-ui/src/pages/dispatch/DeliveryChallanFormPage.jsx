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
  Typography,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
} from '@mui/material';
import { Add, Delete } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  useDeliveryChallan,
  useCreateDeliveryChallan,
  useUpdateDeliveryChallan,
} from '@/hooks/useDeliveryChallans';
import { useProductionWorkOrderLookup } from '@/hooks/useProductionWorkOrders';
import { useWorkOrderLookup } from '@/hooks/useMaterialInwards';
import { useCustomerLookup } from '@/hooks/useCustomers';
import { useSectionProfileLookup } from '@/hooks/useSectionProfiles';

const lineSchema = z.object({
  sectionProfileId: z.number({ required_error: 'Required' }).min(1, 'Required'),
  quantity: z.coerce.number().min(1, 'Min 1'),
  lengthMM: z.coerce.number().min(1, 'Min 1'),
  bundleCount: z.coerce.number().min(0).optional(),
  remarks: z.string().max(500).optional().or(z.literal('')),
});

const schema = z.object({
  date: z.string().min(1, 'Date is required'),
  productionWorkOrderId: z.number({ required_error: 'PWO is required' }).min(1, 'PWO is required'),
  workOrderId: z.number({ required_error: 'WO is required' }).min(1, 'Work Order is required'),
  customerId: z.number({ required_error: 'Customer is required' }).min(1, 'Customer is required'),
  vehicleNumber: z.string().min(1, 'Vehicle number is required').max(20),
  transporterName: z.string().max(200).optional().or(z.literal('')),
  remarks: z.string().max(1000).optional().or(z.literal('')),
  lineItems: z.array(lineSchema).min(1, 'At least one line item is required'),
});

const emptyLine = { sectionProfileId: 0, quantity: 0, lengthMM: 0, bundleCount: 0, remarks: '' };

const defaultValues = {
  date: new Date().toISOString().split('T')[0],
  productionWorkOrderId: 0,
  workOrderId: 0,
  customerId: 0,
  vehicleNumber: '',
  transporterName: '',
  remarks: '',
  lineItems: [{ ...emptyLine }],
};

const STATUS_COLORS = {
  Created: 'default',
  Dispatched: 'success',
  Delivered: 'info',
};

export default function DeliveryChallanFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: existing, isLoading: loadingExisting } = useDeliveryChallan(id);
  const { data: pwos } = useProductionWorkOrderLookup();
  const { data: wos } = useWorkOrderLookup();
  const { data: customers } = useCustomerLookup();
  const { data: sectionProfiles } = useSectionProfileLookup();
  const createMutation = useCreateDeliveryChallan();
  const updateMutation = useUpdateDeliveryChallan();

  const pwoOptions = pwos?.data ?? [];
  const woOptions = wos?.data ?? [];
  const customerOptions = customers?.data ?? [];
  const spOptions = sectionProfiles?.data ?? [];

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

  useEffect(() => {
    if (existing?.data) {
      const d = existing.data;
      reset({
        date: d.date ? new Date(d.date).toISOString().split('T')[0] : '',
        productionWorkOrderId: d.productionWorkOrderId || 0,
        workOrderId: d.workOrderId || 0,
        customerId: d.customerId || 0,
        vehicleNumber: d.vehicleNumber ?? '',
        transporterName: d.transporterName ?? '',
        remarks: d.remarks ?? '',
        lineItems: d.lineItems?.length
          ? d.lineItems.map((l) => ({
              sectionProfileId: l.sectionProfileId || 0,
              quantity: l.quantity ?? 0,
              lengthMM: l.lengthMM ?? 0,
              bundleCount: l.bundleCount ?? 0,
              remarks: l.remarks ?? '',
            }))
          : [{ ...emptyLine }],
      });
    }
  }, [existing, reset]);

  const onSubmit = async (data) => {
    const payload = {
      ...data,
      transporterName: data.transporterName || null,
      remarks: data.remarks || null,
      lineItems: data.lineItems.map((l) => ({
        ...l,
        bundleCount: l.bundleCount || null,
        remarks: l.remarks || null,
      })),
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    try {
      await mutation.mutateAsync(mutationPayload);
      toast.success(`Delivery challan ${isEdit ? 'updated' : 'created'}`);
      navigate('/dispatch/delivery-challans');
    } catch (err) {
      const msg =
        err.response?.data?.errors?.[0] ||
        err.response?.data?.message ||
        'Failed to save delivery challan';
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
        title={isEdit ? `Edit ${existingData?.dcNumber ?? 'Delivery Challan'}` : 'New Delivery Challan'}
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
          {/* PWO */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="productionWorkOrderId"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  size="small"
                  options={pwoOptions}
                  getOptionLabel={(o) => o.name}
                  value={pwoOptions.find((p) => p.id === field.value) ?? null}
                  onChange={(_, val) => field.onChange(val?.id ?? 0)}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Production Work Order"
                      required
                      error={!!errors.productionWorkOrderId}
                      helperText={errors.productionWorkOrderId?.message}
                    />
                  )}
                  isOptionEqualToValue={(opt, val) => opt.id === val.id}
                />
              )}
            />
          </Grid>

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

          {/* Vehicle Number */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="vehicleNumber"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Vehicle Number"
                  required
                  fullWidth
                  size="small"
                  error={!!errors.vehicleNumber}
                  helperText={errors.vehicleNumber?.message}
                />
              )}
            />
          </Grid>

          {/* Transporter Name */}
          <Grid size={{ xs: 12, sm: 4 }}>
            <Controller
              name="transporterName"
              control={control}
              render={({ field }) => (
                <TextField {...field} label="Transporter Name" fullWidth size="small" />
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

        <TableContainer component={Paper} variant="outlined" sx={{ mb: 3 }}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Section Profile</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 100 }}>Qty</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 120 }}>Length (mm)</TableCell>
                <TableCell sx={{ fontWeight: 600, width: 100 }}>Bundles</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Remarks</TableCell>
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
                      name={`lineItems.${index}.quantity`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField
                          {...f}
                          type="number"
                          size="small"
                          error={!!errors.lineItems?.[index]?.quantity}
                          sx={{ width: 80 }}
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
                          sx={{ width: 100 }}
                        />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lineItems.${index}.bundleCount`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField {...f} type="number" size="small" sx={{ width: 80 }} />
                      )}
                    />
                  </TableCell>
                  <TableCell>
                    <Controller
                      name={`lineItems.${index}.remarks`}
                      control={control}
                      render={({ field: f }) => (
                        <TextField {...f} size="small" fullWidth />
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
          <Button variant="outlined" onClick={() => navigate('/dispatch/delivery-challans')}>
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
              'Update Delivery Challan'
            ) : (
              'Save Delivery Challan'
            )}
          </Button>
        </Box>
      </Box>
    </>
  );
}
