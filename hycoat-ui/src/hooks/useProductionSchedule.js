import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { productionScheduleService } from '@/services/ppcService';

export const useProductionSchedule = (params) =>
  useQuery({
    queryKey: ['productionSchedule', params],
    queryFn: () => productionScheduleService.getSchedule(params),
    enabled: !!params?.startDate && !!params?.endDate,
  });

export const useCreateScheduleEntry = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => productionScheduleService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['productionSchedule'] });
      qc.invalidateQueries({ queryKey: ['productionWorkOrders'] });
    },
  });
};

export const useUpdateScheduleEntry = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => productionScheduleService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['productionSchedule'] });
    },
  });
};

export const useUpdateScheduleStatus = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => productionScheduleService.updateStatus(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['productionSchedule'] });
      qc.invalidateQueries({ queryKey: ['productionWorkOrders'] });
    },
  });
};

export const useDeleteScheduleEntry = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => productionScheduleService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['productionSchedule'] });
      qc.invalidateQueries({ queryKey: ['productionWorkOrders'] });
    },
  });
};

export const useReorderSchedule = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => productionScheduleService.reorder(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['productionSchedule'] });
    },
  });
};
