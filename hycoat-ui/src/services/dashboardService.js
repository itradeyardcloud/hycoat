import api from './api';

export const dashboardService = {
  getAdmin: (params) => api.get('/dashboard/admin', { params }).then((r) => r.data),
  getLeader: (params) => api.get('/dashboard/leader', { params }).then((r) => r.data),
  getSales: (params) => api.get('/dashboard/sales', { params }).then((r) => r.data),
  getPpc: (params) => api.get('/dashboard/ppc', { params }).then((r) => r.data),
  getProduction: (params) => api.get('/dashboard/production', { params }).then((r) => r.data),
  getQuality: (params) => api.get('/dashboard/quality', { params }).then((r) => r.data),
  getScm: (params) => api.get('/dashboard/scm', { params }).then((r) => r.data),
  getPurchase: (params) => api.get('/dashboard/purchase', { params }).then((r) => r.data),
  getFinance: (params) => api.get('/dashboard/finance', { params }).then((r) => r.data),
};
