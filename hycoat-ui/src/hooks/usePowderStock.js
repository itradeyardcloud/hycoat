import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { powderStockService } from '@/services/purchaseService';

export const usePowderStock = () =>
  useQuery({
    queryKey: ['powderStock'],
    queryFn: () => powderStockService.getAll(),
  });

export const usePowderStockByColor = (powderColorId) =>
  useQuery({
    queryKey: ['powderStock', powderColorId],
    queryFn: () => powderStockService.getByPowderColorId(powderColorId),
    enabled: !!powderColorId,
  });

export const useLowStock = () =>
  useQuery({
    queryKey: ['powderStock', 'lowStock'],
    queryFn: () => powderStockService.getLowStock(),
  });

export const useUpdateReorderLevel = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ powderColorId, data }) =>
      powderStockService.updateReorderLevel(powderColorId, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['powderStock'] });
    },
  });
};
