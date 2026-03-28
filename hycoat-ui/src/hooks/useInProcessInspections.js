import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { inProcessInspectionService } from '@/services/qualityService';

export const useInProcessInspections = (params) =>
  useQuery({
    queryKey: ['inProcessInspections', params],
    queryFn: () => inProcessInspectionService.getAll(params),
  });

export const useInProcessInspection = (id) =>
  useQuery({
    queryKey: ['inProcessInspections', id],
    queryFn: () => inProcessInspectionService.getById(id),
    enabled: !!id,
  });

export const useInProcessInspectionsByPWO = (pwoId) =>
  useQuery({
    queryKey: ['inProcessInspections', 'byPwo', pwoId],
    queryFn: () => inProcessInspectionService.getByPWO(pwoId),
    enabled: !!pwoId,
  });

export const useDFTTrend = (pwoId) =>
  useQuery({
    queryKey: ['inProcessInspections', 'dftTrend', pwoId],
    queryFn: () => inProcessInspectionService.getDFTTrend(pwoId),
    enabled: !!pwoId,
  });

export const useCreateInProcessInspection = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => inProcessInspectionService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['inProcessInspections'] });
    },
  });
};

export const useUpdateInProcessInspection = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => inProcessInspectionService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['inProcessInspections'] });
    },
  });
};

export const useDeleteInProcessInspection = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => inProcessInspectionService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['inProcessInspections'] });
    },
  });
};
