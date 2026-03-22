import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { vendorService } from '@/services/masterService';

export const useVendors = (params) =>
  useQuery({
    queryKey: ['vendors', params],
    queryFn: () => vendorService.getAll(params),
  });

export const useVendor = (id) =>
  useQuery({
    queryKey: ['vendors', id],
    queryFn: () => vendorService.getById(id),
    enabled: !!id,
  });

export const useVendorLookup = () =>
  useQuery({
    queryKey: ['vendors', 'lookup'],
    queryFn: () => vendorService.lookup(),
    staleTime: 10 * 60 * 1000,
  });

export const useCreateVendor = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => vendorService.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['vendors'] }),
  });
};

export const useUpdateVendor = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => vendorService.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['vendors'] }),
  });
};

export const useDeleteVendor = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => vendorService.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['vendors'] }),
  });
};
