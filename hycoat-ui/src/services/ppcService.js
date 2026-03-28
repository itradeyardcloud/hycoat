import api from './api';

export const productionWorkOrderService = {
  getAll: (params) => api.get('/production-work-orders', { params }).then((r) => r.data),
  getById: (id) => api.get(`/production-work-orders/${id}`).then((r) => r.data),
  create: (data) => api.post('/production-work-orders', data).then((r) => r.data),
  update: (id, data) => api.put(`/production-work-orders/${id}`, data).then((r) => r.data),
  updateStatus: (id, data) =>
    api.patch(`/production-work-orders/${id}/status`, data).then((r) => r.data),
  delete: (id) => api.delete(`/production-work-orders/${id}`),
  calculateTime: (data) =>
    api.post('/production-work-orders/calculate-time', data).then((r) => r.data),
  lookup: (params) =>
    api.get('/production-work-orders/lookup', { params }).then((r) => r.data),
};

export const productionScheduleService = {
  getSchedule: (params) => api.get('/production-schedule', { params }).then((r) => r.data),
  create: (data) => api.post('/production-schedule', data).then((r) => r.data),
  update: (id, data) => api.put(`/production-schedule/${id}`, data).then((r) => r.data),
  updateStatus: (id, data) =>
    api.patch(`/production-schedule/${id}/status`, data).then((r) => r.data),
  delete: (id) => api.delete(`/production-schedule/${id}`),
  reorder: (data) => api.post('/production-schedule/reorder', data).then((r) => r.data),
};
