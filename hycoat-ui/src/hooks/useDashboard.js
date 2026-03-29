import { useQuery } from '@tanstack/react-query';
import { dashboardService } from '@/services/dashboardService';

const REFETCH_INTERVAL = 60_000; // 60 seconds auto-refresh

export const useAdminDashboard = (params) =>
  useQuery({
    queryKey: ['dashboard', 'admin', params],
    queryFn: () => dashboardService.getAdmin(params),
    refetchInterval: REFETCH_INTERVAL,
  });

export const useLeaderDashboard = (params) =>
  useQuery({
    queryKey: ['dashboard', 'leader', params],
    queryFn: () => dashboardService.getLeader(params),
    refetchInterval: REFETCH_INTERVAL,
  });

export const useSalesDashboard = (params) =>
  useQuery({
    queryKey: ['dashboard', 'sales', params],
    queryFn: () => dashboardService.getSales(params),
    refetchInterval: REFETCH_INTERVAL,
  });

export const usePPCDashboard = (params) =>
  useQuery({
    queryKey: ['dashboard', 'ppc', params],
    queryFn: () => dashboardService.getPpc(params),
    refetchInterval: REFETCH_INTERVAL,
  });

export const useProductionDashboard = (params) =>
  useQuery({
    queryKey: ['dashboard', 'production', params],
    queryFn: () => dashboardService.getProduction(params),
    refetchInterval: REFETCH_INTERVAL,
  });

export const useQualityDashboard = (params) =>
  useQuery({
    queryKey: ['dashboard', 'quality', params],
    queryFn: () => dashboardService.getQuality(params),
    refetchInterval: REFETCH_INTERVAL,
  });

export const useSCMDashboard = (params) =>
  useQuery({
    queryKey: ['dashboard', 'scm', params],
    queryFn: () => dashboardService.getScm(params),
    refetchInterval: REFETCH_INTERVAL,
  });

export const usePurchaseDashboard = (params) =>
  useQuery({
    queryKey: ['dashboard', 'purchase', params],
    queryFn: () => dashboardService.getPurchase(params),
    refetchInterval: REFETCH_INTERVAL,
  });

export const useFinanceDashboard = (params) =>
  useQuery({
    queryKey: ['dashboard', 'finance', params],
    queryFn: () => dashboardService.getFinance(params),
    refetchInterval: REFETCH_INTERVAL,
  });
