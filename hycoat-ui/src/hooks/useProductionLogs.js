import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { productionLogService } from '@/services/productionService';

export const useProductionLogs = (params) =>
  useQuery({
    queryKey: ['productionLogs', params],
    queryFn: () => productionLogService.getAll(params),
  });

export const useProductionLog = (id) =>
  useQuery({
    queryKey: ['productionLogs', id],
    queryFn: () => productionLogService.getById(id),
    enabled: !!id,
  });

export const useProductionLogsByPWO = (pwoId) =>
  useQuery({
    queryKey: ['productionLogs', 'byPwo', pwoId],
    queryFn: () => productionLogService.getByPWO(pwoId),
    enabled: !!pwoId,
  });

export const useCreateProductionLog = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => productionLogService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['productionLogs'] });
    },
  });
};

export const useUpdateProductionLog = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => productionLogService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['productionLogs'] });
    },
  });
};

export const useDeleteProductionLog = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => productionLogService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['productionLogs'] });
    },
  });
};

export const useUploadProductionPhoto = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, file, description }) =>
      productionLogService.uploadPhoto(id, file, description),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: ['productionLogs', id] });
    },
  });
};

export const useDeleteProductionPhoto = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, photoId }) => productionLogService.deletePhoto(id, photoId),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: ['productionLogs', id] });
    },
  });
};
