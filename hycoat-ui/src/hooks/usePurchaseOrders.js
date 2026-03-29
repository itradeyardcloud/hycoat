import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { purchaseOrderService } from '@/services/purchaseService';

export const usePurchaseOrders = (params) =>
  useQuery({
    queryKey: ['purchaseOrders', params],
    queryFn: () => purchaseOrderService.getAll(params),
  });

export const usePurchaseOrder = (id) =>
  useQuery({
    queryKey: ['purchaseOrders', id],
    queryFn: () => purchaseOrderService.getById(id),
    enabled: !!id,
  });

export const useCreatePurchaseOrder = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => purchaseOrderService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['purchaseOrders'] });
      qc.invalidateQueries({ queryKey: ['powderIndents'] });
    },
  });
};

export const useUpdatePurchaseOrder = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => purchaseOrderService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['purchaseOrders'] });
    },
  });
};

export const useUpdatePurchaseOrderStatus = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => purchaseOrderService.updateStatus(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['purchaseOrders'] });
    },
  });
};

export const useDeletePurchaseOrder = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => purchaseOrderService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['purchaseOrders'] });
    },
  });
};

export const useGeneratePOPdf = () => {
  return useMutation({
    mutationFn: (id) => purchaseOrderService.generatePdf(id),
  });
};
