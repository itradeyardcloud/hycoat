import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { customerService } from '@/services/masterService';

export const useCustomers = (params) =>
  useQuery({
    queryKey: ['customers', params],
    queryFn: () => customerService.getAll(params),
  });

export const useCustomer = (id) =>
  useQuery({
    queryKey: ['customers', id],
    queryFn: () => customerService.getById(id),
    enabled: !!id,
  });

export const useCustomerLookup = () =>
  useQuery({
    queryKey: ['customers', 'lookup'],
    queryFn: () => customerService.lookup(),
    staleTime: 10 * 60 * 1000,
  });

export const useCreateCustomer = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => customerService.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['customers'] }),
  });
};

export const useUpdateCustomer = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => customerService.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['customers'] }),
  });
};

export const useDeleteCustomer = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => customerService.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['customers'] }),
  });
};
