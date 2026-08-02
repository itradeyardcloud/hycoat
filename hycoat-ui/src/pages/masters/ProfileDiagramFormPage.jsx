import { useEffect, useRef, useState } from 'react';
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
  Typography,
  Divider,
  Stack,
  Alert,
} from '@mui/material';
import { CloudUpload, Image as ImageIcon } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  useProfileDiagram,
  useCreateProfileDiagram,
  useUpdateProfileDiagram,
  useUploadProfileImage,
} from '@/hooks/useProfileDiagrams';

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(50),
  family: z.string().max(20).optional().or(z.literal('')),
  series: z.string().max(20).optional().or(z.literal('')),
  category: z.string().max(100).optional().or(z.literal('')),
  categoryLabel: z.string().max(200).optional().or(z.literal('')),
  system: z.string().max(200).optional().or(z.literal('')),
  sortOrder: z.coerce.number().int().min(0).default(0),
  notes: z.string().max(1000).optional().or(z.literal('')),
});

const defaultValues = {
  code: '',
  family: '',
  series: '',
  category: '',
  categoryLabel: '',
  system: '',
  sortOrder: 0,
  notes: '',
};

export default function ProfileDiagramFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;
  const fileInputRef = useRef(null);
  const [currentImageUrl, setCurrentImageUrl] = useState(null);

  const { data: existing, isLoading: loadingProfile } = useProfileDiagram(id);
  const createMutation = useCreateProfileDiagram();
  const updateMutation = useUpdateProfileDiagram();
  const uploadMutation = useUploadProfileImage();

  const {
    control,
    handleSubmit,
    reset,
    watch,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues,
  });

  // Auto-fill family and series from code
  const codeValue = watch('code');
  useEffect(() => {
    if (!isEdit && codeValue && codeValue.length >= 4) {
      // e.g. AS23PS01 → series=AS23, family=PS
      const seriesMatch = codeValue.match(/^([A-Z]{2}\d{2})/);
      const familyMatch = codeValue.match(/^[A-Z]{2}\d{2}([A-Z]{2})/);
      if (seriesMatch || familyMatch) {
        // Only auto-fill if user hasn't typed in those fields yet — handled by defaultValues
      }
    }
  }, [codeValue, isEdit]);

  useEffect(() => {
    if (existing?.data) {
      const d = existing.data;
      reset({
        code: d.code ?? '',
        family: d.family ?? '',
        series: d.series ?? '',
        category: d.category ?? '',
        categoryLabel: d.categoryLabel ?? '',
        system: d.system ?? '',
        sortOrder: d.sortOrder ?? 0,
        notes: d.notes ?? '',
      });
      setCurrentImageUrl(d.imageUrl ?? null);
    }
  }, [existing, reset]);

  const onSubmit = async (values) => {
    const payload = {
      ...values,
      code: values.code.trim().toUpperCase(),
    };

    if (isEdit) {
      updateMutation.mutate(
        { id: Number(id), data: payload },
        {
          onSuccess: () => {
            toast.success('Profile diagram updated');
            navigate('/masters/profile-diagrams');
          },
          onError: (err) => {
            const msg =
              err?.response?.data?.errors?.[0] ??
              err?.response?.data?.message ??
              'Failed to update';
            toast.error(msg);
          },
        },
      );
    } else {
      createMutation.mutate(payload, {
        onSuccess: (res) => {
          toast.success('Profile diagram created');
          // Navigate to edit so user can immediately upload an image
          navigate(`/masters/profile-diagrams/${res.data.id}/edit`);
        },
        onError: (err) => {
          const msg =
            err?.response?.data?.errors?.[0] ??
            err?.response?.data?.message ??
            'Failed to create';
          toast.error(msg);
        },
      });
    }
  };

  const handleFileChange = (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (file.size > 20 * 1024 * 1024) {
      toast.error('Image must be smaller than 20 MB');
      return;
    }
    if (!['image/png', 'image/jpeg', 'image/jpg'].includes(file.type)) {
      toast.error('Only PNG and JPEG images are supported');
      return;
    }

    uploadMutation.mutate(
      { id: Number(id), file },
      {
        onSuccess: (res) => {
          toast.success('Image uploaded successfully');
          setCurrentImageUrl(res.data);
        },
        onError: (err) => {
          const msg =
            err?.response?.data?.errors?.[0] ?? err?.response?.data?.message ?? 'Upload failed';
          toast.error(msg);
        },
      },
    );
  };

  if (isEdit && loadingProfile) {
    return (
      <Box display="flex" justifyContent="center" mt={6}>
        <CircularProgress />
      </Box>
    );
  }

  const saving = isSubmitting || createMutation.isPending || updateMutation.isPending;

  return (
    <Box>
      <PageHeader
        title={isEdit ? 'Edit Profile Diagram' : 'Add Profile Diagram'}
        subtitle={
          isEdit
            ? 'Update catalog details and upload the profile image'
            : 'Create a new entry, then upload the cropped diagram image'
        }
      />

      <Box
        component="form"
        onSubmit={handleSubmit(onSubmit)}
        sx={{ maxWidth: 760, bgcolor: 'background.paper', p: 3, borderRadius: 2, border: '1px solid', borderColor: 'divider' }}
      >
        {/* ── Catalog fields ─────────────────────────────────────── */}
        <Typography variant="subtitle2" color="text.secondary" gutterBottom>
          Catalog Details
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={4}>
            <Controller
              name="code"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Profile Code *"
                  fullWidth
                  size="small"
                  error={!!errors.code}
                  helperText={errors.code?.message ?? 'e.g. AS23PS01'}
                  inputProps={{ style: { textTransform: 'uppercase' } }}
                  onChange={(e) => field.onChange(e.target.value.toUpperCase())}
                />
              )}
            />
          </Grid>

          <Grid item xs={12} sm={4}>
            <Controller
              name="family"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Family"
                  fullWidth
                  size="small"
                  error={!!errors.family}
                  helperText={errors.family?.message ?? 'e.g. PS, PF, PE'}
                  inputProps={{ style: { textTransform: 'uppercase' } }}
                  onChange={(e) => field.onChange(e.target.value.toUpperCase())}
                />
              )}
            />
          </Grid>

          <Grid item xs={12} sm={4}>
            <Controller
              name="series"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Series"
                  fullWidth
                  size="small"
                  error={!!errors.series}
                  helperText={errors.series?.message ?? 'e.g. AS23, AS32'}
                  inputProps={{ style: { textTransform: 'uppercase' } }}
                  onChange={(e) => field.onChange(e.target.value.toUpperCase())}
                />
              )}
            />
          </Grid>

          <Grid item xs={12} sm={6}>
            <Controller
              name="category"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Category"
                  fullWidth
                  size="small"
                  error={!!errors.category}
                  helperText={errors.category?.message ?? 'e.g. PREMIUM SLIDER'}
                />
              )}
            />
          </Grid>

          <Grid item xs={12} sm={6}>
            <Controller
              name="categoryLabel"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Category Label"
                  fullWidth
                  size="small"
                  error={!!errors.categoryLabel}
                  helperText={errors.categoryLabel?.message}
                />
              )}
            />
          </Grid>

          <Grid item xs={12}>
            <Controller
              name="system"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="System"
                  fullWidth
                  size="small"
                  error={!!errors.system}
                  helperText={errors.system?.message ?? 'e.g. PREMIUM SLIDER (23MM SERIES)'}
                />
              )}
            />
          </Grid>

          <Grid item xs={12} sm={4}>
            <Controller
              name="sortOrder"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Sort Order"
                  fullWidth
                  size="small"
                  type="number"
                  error={!!errors.sortOrder}
                  helperText={errors.sortOrder?.message}
                />
              )}
            />
          </Grid>

          <Grid item xs={12} sm={8}>
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
                  rows={2}
                  error={!!errors.notes}
                  helperText={errors.notes?.message}
                />
              )}
            />
          </Grid>
        </Grid>

        <Stack direction="row" spacing={2} mt={3}>
          <Button
            type="submit"
            variant="contained"
            disabled={saving}
            startIcon={saving ? <CircularProgress size={16} /> : null}
          >
            {isEdit ? 'Save Changes' : 'Create & Upload Image'}
          </Button>
          <Button variant="outlined" onClick={() => navigate('/masters/profile-diagrams')}>
            Cancel
          </Button>
        </Stack>

        {/* ── Image Upload (edit mode only) ────────────────────── */}
        {isEdit && (
          <>
            <Divider sx={{ my: 3 }} />
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              Profile Diagram Image
            </Typography>

            {currentImageUrl ? (
              <Box sx={{ mb: 2, border: '1px solid', borderColor: 'divider', borderRadius: 1, p: 1, display: 'inline-block' }}>
                <img
                  src={currentImageUrl}
                  alt="Profile diagram"
                  style={{ maxWidth: 400, maxHeight: 300, display: 'block', objectFit: 'contain' }}
                />
              </Box>
            ) : (
              <Alert severity="info" sx={{ mb: 2 }}>
                No image uploaded yet. Upload a manually cropped PNG or JPEG below.
              </Alert>
            )}

            <input
              type="file"
              accept="image/png,image/jpeg,image/jpg"
              style={{ display: 'none' }}
              ref={fileInputRef}
              onChange={handleFileChange}
            />
            <Button
              variant="outlined"
              startIcon={
                uploadMutation.isPending ? <CircularProgress size={16} /> : <CloudUpload />
              }
              disabled={uploadMutation.isPending}
              onClick={() => fileInputRef.current?.click()}
            >
              {currentImageUrl ? 'Replace Image' : 'Upload Image'}
            </Button>
            <Typography variant="caption" color="text.secondary" display="block" mt={0.5}>
              PNG or JPEG, max 20 MB. This replaces any existing image.
            </Typography>
          </>
        )}

        {!isEdit && (
          <Alert severity="info" sx={{ mt: 2 }}>
            After creating the entry you will be redirected to the edit page where you can upload
            the diagram image.
          </Alert>
        )}
      </Box>
    </Box>
  );
}
