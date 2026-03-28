import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { panelTestService } from '@/services/qualityService';

export const usePanelTests = (params) =>
  useQuery({
    queryKey: ['panelTests', params],
    queryFn: () => panelTestService.getAll(params),
  });

export const usePanelTest = (id) =>
  useQuery({
    queryKey: ['panelTests', id],
    queryFn: () => panelTestService.getById(id),
    enabled: !!id,
  });

export const usePanelTestsByPWO = (pwoId) =>
  useQuery({
    queryKey: ['panelTests', 'byPwo', pwoId],
    queryFn: () => panelTestService.getByPWO(pwoId),
    enabled: !!pwoId,
  });

export const useCreatePanelTest = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => panelTestService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['panelTests'] });
    },
  });
};

export const useUpdatePanelTest = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => panelTestService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['panelTests'] });
    },
  });
};

export const useDeletePanelTest = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => panelTestService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['panelTests'] });
    },
  });
};
