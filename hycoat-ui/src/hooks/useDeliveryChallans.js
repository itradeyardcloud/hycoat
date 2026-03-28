import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { deliveryChallanService } from '@/services/dispatchService';

export const useDeliveryChallans = (params) =>
  useQuery({
    queryKey: ['deliveryChallans', params],
    queryFn: () => deliveryChallanService.getAll(params),
  });

export const useDeliveryChallan = (id) =>
  useQuery({
    queryKey: ['deliveryChallans', id],
    queryFn: () => deliveryChallanService.getById(id),
    enabled: !!id,
  });

export const useCreateDeliveryChallan = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => deliveryChallanService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['deliveryChallans'] });
    },
  });
};

export const useUpdateDeliveryChallan = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => deliveryChallanService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['deliveryChallans'] });
    },
  });
};

export const useDeleteDeliveryChallan = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => deliveryChallanService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['deliveryChallans'] });
    },
  });
};

export const useUpdateDCStatus = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }) => deliveryChallanService.updateStatus(id, { status }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['deliveryChallans'] });
    },
  });
};

export const useUploadLoadingPhotos = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, files }) => deliveryChallanService.uploadLoadingPhotos(id, files),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['deliveryChallans'] });
    },
  });
};

export const useGenerateDCPdf = () =>
  useMutation({
    mutationFn: (id) => deliveryChallanService.generatePdf(id),
  });

export const useDownloadDCPdf = () =>
  useMutation({
    mutationFn: (id) => deliveryChallanService.downloadPdf(id),
  });
