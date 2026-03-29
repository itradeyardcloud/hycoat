import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { grnService } from '@/services/purchaseService';

export const useGRNs = (params) =>
  useQuery({
    queryKey: ['grns', params],
    queryFn: () => grnService.getAll(params),
  });

export const useGRN = (id) =>
  useQuery({
    queryKey: ['grns', id],
    queryFn: () => grnService.getById(id),
    enabled: !!id,
  });

export const useCreateGRN = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => grnService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['grns'] });
      qc.invalidateQueries({ queryKey: ['purchaseOrders'] });
      qc.invalidateQueries({ queryKey: ['powderIndents'] });
      qc.invalidateQueries({ queryKey: ['powderStock'] });
    },
  });
};

export const useUpdateGRN = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => grnService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['grns'] });
      qc.invalidateQueries({ queryKey: ['purchaseOrders'] });
      qc.invalidateQueries({ queryKey: ['powderStock'] });
    },
  });
};

export const useDeleteGRN = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => grnService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['grns'] });
      qc.invalidateQueries({ queryKey: ['purchaseOrders'] });
      qc.invalidateQueries({ queryKey: ['powderStock'] });
    },
  });
};
