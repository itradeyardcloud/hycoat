import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { pretreatmentLogService } from '@/services/productionService';

export const usePretreatmentLogs = (params) =>
  useQuery({
    queryKey: ['pretreatmentLogs', params],
    queryFn: () => pretreatmentLogService.getAll(params),
  });

export const usePretreatmentLog = (id) =>
  useQuery({
    queryKey: ['pretreatmentLogs', id],
    queryFn: () => pretreatmentLogService.getById(id),
    enabled: !!id,
  });

export const usePretreatmentLogsByPWO = (pwoId) =>
  useQuery({
    queryKey: ['pretreatmentLogs', 'byPwo', pwoId],
    queryFn: () => pretreatmentLogService.getByPWO(pwoId),
    enabled: !!pwoId,
  });

export const useCreatePretreatmentLog = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => pretreatmentLogService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['pretreatmentLogs'] });
    },
  });
};

export const useUpdatePretreatmentLog = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => pretreatmentLogService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['pretreatmentLogs'] });
    },
  });
};

export const useDeletePretreatmentLog = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => pretreatmentLogService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['pretreatmentLogs'] });
    },
  });
};

export const useAddTankReadings = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, readings }) => pretreatmentLogService.addTankReadings(id, readings),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: ['pretreatmentLogs', id] });
    },
  });
};
