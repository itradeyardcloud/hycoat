import api from './api';

export const yieldReportService = {
  getAll: (params) => api.get('/yield-reports', { params }).then((r) => r.data),
  getSummary: (params) => api.get('/yield-reports/summary', { params }).then((r) => r.data),
  create: (data) => api.post('/yield-reports', data).then((r) => r.data),
  delete: (id) => api.delete(`/yield-reports/${id}`),
};