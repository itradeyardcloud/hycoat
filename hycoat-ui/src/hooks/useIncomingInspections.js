import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { incomingInspectionService } from '@/services/materialInwardService';

export const useIncomingInspections = (params) =>
  useQuery({
    queryKey: ['incomingInspections', params],
    queryFn: () => incomingInspectionService.getAll(params),
  });

export const useIncomingInspection = (id) =>
  useQuery({
    queryKey: ['incomingInspections', id],
    queryFn: () => incomingInspectionService.getById(id),
    enabled: !!id,
  });

export const useCreateIncomingInspection = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => incomingInspectionService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['incomingInspections'] });
      qc.invalidateQueries({ queryKey: ['materialInwards'] });
    },
  });
};

export const useUpdateIncomingInspection = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => incomingInspectionService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['incomingInspections'] });
    },
  });
};

export const useDeleteIncomingInspection = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => incomingInspectionService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['incomingInspections'] });
    },
  });
};

export const useUploadInspectionPhotos = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, files }) => incomingInspectionService.uploadPhotos(id, files),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: ['incomingInspections', id] });
    },
  });
};
