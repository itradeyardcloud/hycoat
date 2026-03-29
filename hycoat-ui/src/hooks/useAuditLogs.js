import { useQuery } from '@tanstack/react-query';
import { auditLogService } from '@/services/auditLogService';

export const useAuditLogs = (params) =>
  useQuery({
    queryKey: ['audit-logs', params],
    queryFn: () => auditLogService.getAll(params),
  });

export const useEntityAuditLogs = (entityName, entityId, params) =>
  useQuery({
    queryKey: ['audit-logs', entityName, entityId, params],
    queryFn: () => auditLogService.getByEntity(entityName, entityId, params),
    enabled: !!entityName && !!entityId,
  });
