import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { finalInspectionService } from '@/services/qualityService';

export const useFinalInspections = (params) =>
  useQuery({
    queryKey: ['finalInspections', params],
    queryFn: () => finalInspectionService.getAll(params),
  });

export const useFinalInspection = (id) =>
  useQuery({
    queryKey: ['finalInspections', id],
    queryFn: () => finalInspectionService.getById(id),
    enabled: !!id,
  });

export const useFinalInspectionByPWO = (pwoId) =>
  useQuery({
    queryKey: ['finalInspections', 'byPwo', pwoId],
    queryFn: () => finalInspectionService.getByPWO(pwoId),
    enabled: !!pwoId,
  });

export const useCreateFinalInspection = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => finalInspectionService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['finalInspections'] });
    },
  });
};

export const useUpdateFinalInspection = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => finalInspectionService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['finalInspections'] });
    },
  });
};

export const useDeleteFinalInspection = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => finalInspectionService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['finalInspections'] });
    },
  });
};
