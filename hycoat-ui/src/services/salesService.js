import api from './api';

export const inquiryService = {
  getAll: (params) => api.get('/inquiries', { params }).then((r) => r.data),
  getById: (id) => api.get(`/inquiries/${id}`).then((r) => r.data),
  create: (data) => api.post('/inquiries', data).then((r) => r.data),
  update: (id, data) => api.put(`/inquiries/${id}`, data).then((r) => r.data),
  updateStatus: (id, data) => api.patch(`/inquiries/${id}/status`, data).then((r) => r.data),
  delete: (id) => api.delete(`/inquiries/${id}`),
  getStats: () => api.get('/inquiries/stats').then((r) => r.data),
  lookup: () => api.get('/inquiries/lookup').then((r) => r.data),
};
