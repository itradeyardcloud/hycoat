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

export const quotationService = {
  getAll: (params) => api.get('/quotations', { params }).then((r) => r.data),
  getById: (id) => api.get(`/quotations/${id}`).then((r) => r.data),
  create: (data) => api.post('/quotations', data).then((r) => r.data),
  update: (id, data) => api.put(`/quotations/${id}`, data).then((r) => r.data),
  updateStatus: (id, data) => api.patch(`/quotations/${id}/status`, data).then((r) => r.data),
  delete: (id) => api.delete(`/quotations/${id}`),
  getPdf: (id) => api.get(`/quotations/${id}/pdf`, { responseType: 'blob' }),
  lookup: () => api.get('/quotations/lookup').then((r) => r.data),
};

export const piService = {
  getAll: (params) => api.get('/proforma-invoices', { params }).then((r) => r.data),
  getById: (id) => api.get(`/proforma-invoices/${id}`).then((r) => r.data),
  create: (data) => api.post('/proforma-invoices', data).then((r) => r.data),
  update: (id, data) => api.put(`/proforma-invoices/${id}`, data).then((r) => r.data),
  updateStatus: (id, data) => api.patch(`/proforma-invoices/${id}/status`, data).then((r) => r.data),
  delete: (id) => api.delete(`/proforma-invoices/${id}`),
  getPdf: (id) => api.get(`/proforma-invoices/${id}/pdf`, { responseType: 'blob' }),
  calculateArea: (data) => api.post('/proforma-invoices/calculate-area', data).then((r) => r.data),
  lookup: () => api.get('/proforma-invoices/lookup').then((r) => r.data),
};

export const workOrderService = {
  getAll: (params) => api.get('/work-orders', { params }).then((r) => r.data),
  getById: (id) => api.get(`/work-orders/${id}`).then((r) => r.data),
  create: (data) => api.post('/work-orders', data).then((r) => r.data),
  update: (id, data) => api.put(`/work-orders/${id}`, data).then((r) => r.data),
  updateStatus: (id, data) => api.patch(`/work-orders/${id}/status`, data).then((r) => r.data),
  delete: (id) => api.delete(`/work-orders/${id}`),
  getStats: () => api.get('/work-orders/stats').then((r) => r.data),
  getTimeline: (id) => api.get(`/work-orders/${id}/timeline`).then((r) => r.data),
  lookup: () => api.get('/work-orders/lookup').then((r) => r.data),
};
