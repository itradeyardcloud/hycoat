import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { productionUnitService } from '@/services/masterService';

export const useProductionUnits = () =>
  useQuery({
    queryKey: ['productionUnits'],
    queryFn: () => productionUnitService.getAll(),
    staleTime: 10 * 60 * 1000,
  });

export const useProductionUnit = (id) =>
  useQuery({
    queryKey: ['productionUnits', id],
    queryFn: () => productionUnitService.getById(id),
    enabled: !!id,
  });

export const useCreateProductionUnit = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => productionUnitService.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['productionUnits'] }),
  });
};

export const useUpdateProductionUnit = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => productionUnitService.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['productionUnits'] }),
  });
};

export const useDeleteProductionUnit = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => productionUnitService.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['productionUnits'] }),
  });
};
