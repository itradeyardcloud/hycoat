import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { materialInwardService, workOrderService } from '@/services/materialInwardService';

export const useWorkOrderLookup = (params) =>
  useQuery({
    queryKey: ['workOrders', 'lookup', params],
    queryFn: () => workOrderService.lookup(params),
    staleTime: 2 * 60 * 1000,
  });

export const useMaterialInwards = (params) =>
  useQuery({
    queryKey: ['materialInwards', params],
    queryFn: () => materialInwardService.getAll(params),
  });

export const useMaterialInward = (id) =>
  useQuery({
    queryKey: ['materialInwards', id],
    queryFn: () => materialInwardService.getById(id),
    enabled: !!id,
  });

export const useMaterialInwardLookup = (params) =>
  useQuery({
    queryKey: ['materialInwards', 'lookup', params],
    queryFn: () => materialInwardService.lookup(params),
    staleTime: 5 * 60 * 1000,
  });

export const useCreateMaterialInward = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => materialInwardService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['materialInwards'] });
    },
  });
};

export const useUpdateMaterialInward = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => materialInwardService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['materialInwards'] });
    },
  });
};

export const useUpdateMaterialInwardStatus = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }) => materialInwardService.updateStatus(id, { status }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['materialInwards'] });
    },
  });
};

export const useDeleteMaterialInward = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => materialInwardService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['materialInwards'] });
    },
  });
};

export const useUploadMaterialInwardPhotos = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, files }) => materialInwardService.uploadPhotos(id, files),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: ['materialInwards', id] });
    },
  });
};

export const useDeleteMaterialInwardPhoto = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, photoId }) => materialInwardService.deletePhoto(id, photoId),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: ['materialInwards', id] });
    },
  });
};
