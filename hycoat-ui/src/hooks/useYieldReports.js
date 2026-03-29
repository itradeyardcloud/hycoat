import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { yieldReportService } from '@/services/yieldReportService';

export const useYieldReports = (params) =>
  useQuery({
    queryKey: ['yieldReports', params],
    queryFn: () => yieldReportService.getAll(params),
  });

export const useYieldSummary = (params) =>
  useQuery({
    queryKey: ['yieldReports', 'summary', params],
    queryFn: () => yieldReportService.getSummary(params),
  });

export const useCreateYieldReport = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => yieldReportService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['yieldReports'] });
    },
  });
};

export const useDeleteYieldReport = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => yieldReportService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['yieldReports'] });
    },
  });
};