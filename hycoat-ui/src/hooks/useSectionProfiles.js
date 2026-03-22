import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { sectionProfileService } from '@/services/masterService';

export const useSectionProfiles = (params) =>
  useQuery({
    queryKey: ['sectionProfiles', params],
    queryFn: () => sectionProfileService.getAll(params),
  });

export const useSectionProfile = (id) =>
  useQuery({
    queryKey: ['sectionProfiles', id],
    queryFn: () => sectionProfileService.getById(id),
    enabled: !!id,
  });

export const useSectionProfileLookup = () =>
  useQuery({
    queryKey: ['sectionProfiles', 'lookup'],
    queryFn: () => sectionProfileService.lookup(),
    staleTime: 10 * 60 * 1000,
  });

export const useCreateSectionProfile = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => sectionProfileService.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sectionProfiles'] }),
  });
};

export const useUpdateSectionProfile = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => sectionProfileService.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sectionProfiles'] }),
  });
};

export const useDeleteSectionProfile = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => sectionProfileService.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sectionProfiles'] }),
  });
};

export const useUploadDrawing = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, file }) => sectionProfileService.uploadDrawing(id, file),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sectionProfiles'] }),
  });
};
