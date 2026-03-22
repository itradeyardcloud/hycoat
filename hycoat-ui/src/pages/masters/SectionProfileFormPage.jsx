import { useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Box, Button, TextField, Grid, CircularProgress, Typography } from '@mui/material';
import { CloudUpload } from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import {
  useSectionProfile,
  useCreateSectionProfile,
  useUpdateSectionProfile,
  useUploadDrawing,
} from '@/hooks/useSectionProfiles';

const schema = z.object({
  sectionNumber: z.string().min(1, 'Section number is required').max(50),
  type: z.string().max(100).optional().or(z.literal('')),
  perimeterMM: z.coerce.number().positive('Perimeter must be greater than 0'),
  weightPerMeter: z.coerce.number().positive().optional().or(z.literal('')),
  heightMM: z.coerce.number().positive().optional().or(z.literal('')),
  widthMM: z.coerce.number().positive().optional().or(z.literal('')),
  thicknessMM: z.coerce.number().positive().optional().or(z.literal('')),
  notes: z.string().max(1000).optional().or(z.literal('')),
});

const defaultValues = {
  sectionNumber: '',
  type: '',
  perimeterMM: '',
  weightPerMeter: '',
  heightMM: '',
  widthMM: '',
  thicknessMM: '',
  notes: '',
};

export default function SectionProfileFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;
  const fileInputRef = useRef(null);
  const [selectedFile, setSelectedFile] = useState(null);
  const [previewUrl, setPreviewUrl] = useState(null);

  const { data: existing, isLoading: loadingProfile } = useSectionProfile(id);
  const createMutation = useCreateSectionProfile();
  const updateMutation = useUpdateSectionProfile();
  const uploadMutation = useUploadDrawing();

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
        sectionNumber: existing.data.sectionNumber ?? '',
        type: existing.data.type ?? '',
        perimeterMM: existing.data.perimeterMM ?? '',
        weightPerMeter: existing.data.weightPerMeter ?? '',
        heightMM: existing.data.heightMM ?? '',
        widthMM: existing.data.widthMM ?? '',
        thicknessMM: existing.data.thicknessMM ?? '',
        notes: existing.data.notes ?? '',
      });
      if (existing.data.drawingFileUrl) {
        setPreviewUrl(existing.data.drawingFileUrl);
      }
    }
  }, [existing, reset]);

  const handleFileChange = (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (file.size > 10 * 1024 * 1024) {
      toast.error('File must be less than 10MB');
      return;
    }

    const validTypes = ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf'];
    if (!validTypes.includes(file.type)) {
      toast.error('Only JPG, PNG, and PDF files are allowed');
      return;
    }

    setSelectedFile(file);
    if (file.type.startsWith('image/')) {
      setPreviewUrl(URL.createObjectURL(file));
    } else {
      setPreviewUrl(null);
    }
  };

  const onSubmit = async (data) => {
    // Convert empty strings to null for optional number fields
    const payload = {
      ...data,
      weightPerMeter: data.weightPerMeter || null,
      heightMM: data.heightMM || null,
      widthMM: data.widthMM || null,
      thicknessMM: data.thicknessMM || null,
    };

    const mutation = isEdit ? updateMutation : createMutation;
    const mutationPayload = isEdit ? { id: Number(id), data: payload } : payload;

    mutation.mutate(mutationPayload, {
      onSuccess: (result) => {
        const profileId = isEdit ? Number(id) : result?.data?.id;
        if (selectedFile && profileId) {
          uploadMutation.mutate(
            { id: profileId, file: selectedFile },
            {
              onSuccess: () => {
                toast.success(`Section profile ${isEdit ? 'updated' : 'created'}`);
                navigate('/masters/section-profiles');
              },
              onError: () => {
                toast.success(`Section profile ${isEdit ? 'updated' : 'created'} but drawing upload failed`);
                navigate('/masters/section-profiles');
              },
            },
          );
        } else {
          toast.success(`Section profile ${isEdit ? 'updated' : 'created'}`);
          navigate('/masters/section-profiles');
        }
      },
      onError: (err) => {
        const msg = err.response?.data?.errors?.[0] || err.response?.data?.message || 'Failed to save section profile';
        toast.error(msg);
      },
    });
  };

  const isSaving = isSubmitting || createMutation.isPending || updateMutation.isPending || uploadMutation.isPending;

  if (isEdit && loadingProfile) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <>
      <PageHeader title={isEdit ? 'Edit Section Profile' : 'Add Section Profile'} />

      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ maxWidth: 800 }}>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Field control={control} name="sectionNumber" label="Section Number" required errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <Field control={control} name="type" label="Type" errors={errors} />
          </Grid>

          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="perimeterMM" label="Perimeter (mm)" required type="number" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="weightPerMeter" label="Weight/m (kg)" type="number" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="heightMM" label="Height (mm)" type="number" errors={errors} />
          </Grid>

          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="widthMM" label="Width (mm)" type="number" errors={errors} />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <Field control={control} name="thicknessMM" label="Thickness (mm)" type="number" errors={errors} />
          </Grid>

          <Grid size={12}>
            <Field control={control} name="notes" label="Notes" multiline rows={3} errors={errors} />
          </Grid>

          {/* Drawing Upload */}
          <Grid size={12}>
            <Typography variant="subtitle2" sx={{ mb: 1 }}>
              Drawing
            </Typography>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/jpeg,image/png,image/jpg,application/pdf"
              hidden
              onChange={handleFileChange}
            />
            <Button variant="outlined" startIcon={<CloudUpload />} onClick={() => fileInputRef.current?.click()}>
              {selectedFile ? selectedFile.name : 'Upload Drawing'}
            </Button>
            {previewUrl && (
              <Box sx={{ mt: 1 }}>
                <img
                  src={previewUrl}
                  alt="Drawing preview"
                  style={{ maxWidth: 200, maxHeight: 200, objectFit: 'contain', borderRadius: 4 }}
                />
              </Box>
            )}
          </Grid>

          <Grid size={12}>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 1 }}>
              <Button variant="outlined" onClick={() => navigate('/masters/section-profiles')}>
                Cancel
              </Button>
              <Button type="submit" variant="contained" disabled={isSaving}>
                {isSaving ? <CircularProgress size={22} /> : 'Save Section Profile'}
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
