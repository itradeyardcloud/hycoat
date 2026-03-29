import api from './api';

export const auditLogService = {
  getAll: (params) => api.get('/audit-logs', { params }).then((r) => r.data),
  getByEntity: (entityName, entityId, params) =>
    api.get(`/audit-logs/entity/${entityName}/${entityId}`, { params }).then((r) => r.data),
};
