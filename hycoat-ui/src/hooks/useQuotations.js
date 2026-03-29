import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { quotationService } from '@/services/salesService';

export const useQuotations = (params) =>
  useQuery({
    queryKey: ['quotations', params],
    queryFn: () => quotationService.getAll(params),
  });

export const useQuotation = (id) =>
  useQuery({
    queryKey: ['quotations', id],
    queryFn: () => quotationService.getById(id),
    enabled: !!id,
  });

export const useQuotationLookup = () =>
  useQuery({
    queryKey: ['quotations', 'lookup'],
    queryFn: () => quotationService.lookup(),
    staleTime: 5 * 60 * 1000,
  });

export const useCreateQuotation = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => quotationService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['quotations'] });
    },
  });
};

export const useUpdateQuotation = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => quotationService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['quotations'] });
    },
  });
};

export const useUpdateQuotationStatus = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }) => quotationService.updateStatus(id, { status }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['quotations'] });
    },
  });
};

export const useDeleteQuotation = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => quotationService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['quotations'] });
    },
  });
};
