import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { workOrderService } from '@/services/salesService';

export const useWorkOrders = (params) =>
  useQuery({
    queryKey: ['workOrders', params],
    queryFn: () => workOrderService.getAll(params),
  });

export const useWorkOrder = (id) =>
  useQuery({
    queryKey: ['workOrders', id],
    queryFn: () => workOrderService.getById(id),
    enabled: !!id,
  });

export const useWorkOrderStats = () =>
  useQuery({
    queryKey: ['workOrders', 'stats'],
    queryFn: () => workOrderService.getStats(),
  });

export const useWorkOrderTimeline = (id) =>
  useQuery({
    queryKey: ['workOrders', 'timeline', id],
    queryFn: () => workOrderService.getTimeline(id),
    enabled: !!id,
  });

export const useWorkOrderLookup = () =>
  useQuery({
    queryKey: ['workOrders', 'lookup'],
    queryFn: () => workOrderService.lookup(),
    staleTime: 5 * 60 * 1000,
  });

export const useCreateWorkOrder = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => workOrderService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['workOrders'] });
    },
  });
};

export const useUpdateWorkOrder = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => workOrderService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['workOrders'] });
    },
  });
};

export const useUpdateWorkOrderStatus = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }) => workOrderService.updateStatus(id, { status }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['workOrders'] });
    },
  });
};

export const useDeleteWorkOrder = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => workOrderService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['workOrders'] });
    },
  });
};
