import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { packingListService } from '@/services/dispatchService';

export const usePackingLists = (params) =>
  useQuery({
    queryKey: ['packingLists', params],
    queryFn: () => packingListService.getAll(params),
  });

export const usePackingList = (id) =>
  useQuery({
    queryKey: ['packingLists', id],
    queryFn: () => packingListService.getById(id),
    enabled: !!id,
  });

export const usePackingListByWorkOrder = (woId) =>
  useQuery({
    queryKey: ['packingLists', 'byWorkOrder', woId],
    queryFn: () => packingListService.getByWorkOrder(woId),
    enabled: !!woId,
  });

export const useCreatePackingList = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => packingListService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['packingLists'] });
    },
  });
};

export const useUpdatePackingList = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => packingListService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['packingLists'] });
    },
  });
};

export const useDeletePackingList = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => packingListService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['packingLists'] });
    },
  });
};
