import { useQuery, useMutation } from '@tanstack/react-query';
import { reportService } from '@/services/reportService';
import { downloadBlob } from '@/utils/downloadBlob';

export const useOrderTracker = (params) =>
  useQuery({
    queryKey: ['reports', 'order-tracker', params],
    queryFn: () => reportService.getOrderTracker(params),
  });

export const useProductionThroughput = (params) =>
  useQuery({
    queryKey: ['reports', 'production-throughput', params],
    queryFn: () => reportService.getProductionThroughput(params),
  });

export const usePowderConsumption = (params) =>
  useQuery({
    queryKey: ['reports', 'powder-consumption', params],
    queryFn: () => reportService.getPowderConsumption(params),
  });

export const useQualitySummary = (params) =>
  useQuery({
    queryKey: ['reports', 'quality-summary', params],
    queryFn: () => reportService.getQualitySummary(params),
  });

export const useCustomerHistory = (customerId) =>
  useQuery({
    queryKey: ['reports', 'customer-history', customerId],
    queryFn: () => reportService.getCustomerHistory(customerId),
    enabled: !!customerId,
  });

export const useDispatchRegister = (params) =>
  useQuery({
    queryKey: ['reports', 'dispatch-register', params],
    queryFn: () => reportService.getDispatchRegister(params),
  });

export const useExportReport = () =>
  useMutation({
    mutationFn: async ({ type, params }) => {
      const exportFns = {
        'order-tracker': reportService.exportOrderTracker,
        'production-throughput': reportService.exportProductionThroughput,
        'powder-consumption': reportService.exportPowderConsumption,
        'dispatch-register': reportService.exportDispatchRegister,
      };
      const fn = exportFns[type];
      if (!fn) throw new Error(`Unknown report type: ${type}`);
      const response = await fn(params);
      downloadBlob(response.data, `${type}.xlsx`);
    },
  });
