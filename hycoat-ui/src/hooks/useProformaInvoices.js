import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { piService } from '@/services/salesService';

export const useProformaInvoices = (params) =>
  useQuery({
    queryKey: ['proformaInvoices', params],
    queryFn: () => piService.getAll(params),
  });

export const useProformaInvoice = (id) =>
  useQuery({
    queryKey: ['proformaInvoices', id],
    queryFn: () => piService.getById(id),
    enabled: !!id,
  });

export const usePILookup = () =>
  useQuery({
    queryKey: ['proformaInvoices', 'lookup'],
    queryFn: () => piService.lookup(),
    staleTime: 5 * 60 * 1000,
  });

export const useCreatePI = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => piService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['proformaInvoices'] });
    },
  });
};

export const useUpdatePI = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => piService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['proformaInvoices'] });
    },
  });
};

export const useUpdatePIStatus = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }) => piService.updateStatus(id, { status }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['proformaInvoices'] });
    },
  });
};

export const useDeletePI = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => piService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['proformaInvoices'] });
    },
  });
};

export const useCalculateArea = () =>
  useMutation({
    mutationFn: (data) => piService.calculateArea(data),
  });
