import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { inquiryService } from '@/services/salesService';

export const useInquiries = (params) =>
  useQuery({
    queryKey: ['inquiries', params],
    queryFn: () => inquiryService.getAll(params),
  });

export const useInquiry = (id) =>
  useQuery({
    queryKey: ['inquiries', id],
    queryFn: () => inquiryService.getById(id),
    enabled: !!id,
  });

export const useInquiryStats = () =>
  useQuery({
    queryKey: ['inquiries', 'stats'],
    queryFn: () => inquiryService.getStats(),
  });

export const useInquiryLookup = () =>
  useQuery({
    queryKey: ['inquiries', 'lookup'],
    queryFn: () => inquiryService.lookup(),
    staleTime: 5 * 60 * 1000,
  });

export const useCreateInquiry = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => inquiryService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['inquiries'] });
    },
  });
};

export const useUpdateInquiry = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => inquiryService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['inquiries'] });
    },
  });
};

export const useUpdateInquiryStatus = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }) => inquiryService.updateStatus(id, { status }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['inquiries'] });
    },
  });
};

export const useDeleteInquiry = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => inquiryService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['inquiries'] });
    },
  });
};
