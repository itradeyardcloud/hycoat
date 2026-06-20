import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { powderColorService } from '@/services/masterService';

export const usePowderColors = (params) =>
  useQuery({
    queryKey: ['powderColors', params],
    queryFn: () => powderColorService.getAll(params),
  });

export const usePowderColor = (id) =>
  useQuery({
    queryKey: ['powderColors', id],
    queryFn: () => powderColorService.getById(id),
    enabled: !!id,
  });

export const usePowderColorLookup = () =>
  useQuery({
    queryKey: ['powderColors', 'lookup'],
    queryFn: () => powderColorService.lookup(),
    select: (response) => ({
      ...response,
      data: (response?.data ?? []).map((item) => ({
        ...item,
        name: item.name ?? item.colorName ?? item.powderCode ?? '',
      })),
    }),
    staleTime: 10 * 60 * 1000,
  });

export const useCreatePowderColor = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => powderColorService.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['powderColors'] }),
  });
};

export const useUpdatePowderColor = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => powderColorService.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['powderColors'] }),
  });
};

export const useDeletePowderColor = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => powderColorService.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['powderColors'] }),
  });
};
