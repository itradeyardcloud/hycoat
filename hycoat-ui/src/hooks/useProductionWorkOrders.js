import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { productionWorkOrderService } from '@/services/ppcService';

export const useProductionWorkOrders = (params) =>
  useQuery({
    queryKey: ['productionWorkOrders', params],
    queryFn: () => productionWorkOrderService.getAll(params),
  });

export const useProductionWorkOrder = (id) =>
  useQuery({
    queryKey: ['productionWorkOrders', id],
    queryFn: () => productionWorkOrderService.getById(id),
    enabled: !!id,
  });

export const useProductionWorkOrderLookup = (params) =>
  useQuery({
    queryKey: ['productionWorkOrders', 'lookup', params],
    queryFn: () => productionWorkOrderService.lookup(params),
    staleTime: 2 * 60 * 1000,
  });

export const useCreateProductionWorkOrder = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => productionWorkOrderService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['productionWorkOrders'] });
      qc.invalidateQueries({ queryKey: ['workOrders'] });
    },
  });
};

export const useUpdateProductionWorkOrder = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => productionWorkOrderService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['productionWorkOrders'] });
    },
  });
};

export const useUpdatePWOStatus = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => productionWorkOrderService.updateStatus(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['productionWorkOrders'] });
    },
  });
};

export const useDeleteProductionWorkOrder = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => productionWorkOrderService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['productionWorkOrders'] });
    },
  });
};

export const useCalculateProductionTime = () => {
  return useMutation({
    mutationFn: (data) => productionWorkOrderService.calculateTime(data),
  });
};
