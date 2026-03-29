import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { powderIndentService } from '@/services/purchaseService';

export const usePowderIndents = (params) =>
  useQuery({
    queryKey: ['powderIndents', params],
    queryFn: () => powderIndentService.getAll(params),
  });

export const usePowderIndent = (id) =>
  useQuery({
    queryKey: ['powderIndents', id],
    queryFn: () => powderIndentService.getById(id),
    enabled: !!id,
  });

export const useCreatePowderIndent = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => powderIndentService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['powderIndents'] });
    },
  });
};

export const useUpdatePowderIndent = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => powderIndentService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['powderIndents'] });
    },
  });
};

export const useUpdatePowderIndentStatus = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => powderIndentService.updateStatus(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['powderIndents'] });
    },
  });
};

export const useDeletePowderIndent = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => powderIndentService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['powderIndents'] });
    },
  });
};
