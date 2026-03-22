import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { processTypeService } from '@/services/masterService';

export const useProcessTypes = () =>
  useQuery({
    queryKey: ['processTypes'],
    queryFn: () => processTypeService.getAll(),
    staleTime: 10 * 60 * 1000,
  });

export const useProcessType = (id) =>
  useQuery({
    queryKey: ['processTypes', id],
    queryFn: () => processTypeService.getById(id),
    enabled: !!id,
  });

export const useCreateProcessType = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => processTypeService.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['processTypes'] }),
  });
};

export const useUpdateProcessType = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => processTypeService.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['processTypes'] }),
  });
};

export const useDeleteProcessType = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => processTypeService.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['processTypes'] }),
  });
};
