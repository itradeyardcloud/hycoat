import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { testCertificateService } from '@/services/qualityService';

export const useTestCertificates = (params) =>
  useQuery({
    queryKey: ['testCertificates', params],
    queryFn: () => testCertificateService.getAll(params),
  });

export const useTestCertificate = (id) =>
  useQuery({
    queryKey: ['testCertificates', id],
    queryFn: () => testCertificateService.getById(id),
    enabled: !!id,
  });

export const useTestCertificateByWorkOrder = (woId) =>
  useQuery({
    queryKey: ['testCertificates', 'byWorkOrder', woId],
    queryFn: () => testCertificateService.getByWorkOrder(woId),
    enabled: !!woId,
  });

export const useCreateTestCertificate = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => testCertificateService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['testCertificates'] });
    },
  });
};

export const useUpdateTestCertificate = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => testCertificateService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['testCertificates'] });
    },
  });
};

export const useDeleteTestCertificate = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => testCertificateService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['testCertificates'] });
    },
  });
};

export const useGenerateTestCertificatePdf = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => testCertificateService.generatePdf(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['testCertificates'] });
    },
  });
};
