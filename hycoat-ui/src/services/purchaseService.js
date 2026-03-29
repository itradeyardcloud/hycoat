import api from './api';

export const powderIndentService = {
  getAll: (params) => api.get('/powder-indents', { params }).then((r) => r.data),
  getById: (id) => api.get(`/powder-indents/${id}`).then((r) => r.data),
  create: (data) => api.post('/powder-indents', data).then((r) => r.data),
  update: (id, data) => api.put(`/powder-indents/${id}`, data).then((r) => r.data),
  updateStatus: (id, data) => api.patch(`/powder-indents/${id}/status`, data).then((r) => r.data),
  delete: (id) => api.delete(`/powder-indents/${id}`),
};

export const purchaseOrderService = {
  getAll: (params) => api.get('/purchase-orders', { params }).then((r) => r.data),
  getById: (id) => api.get(`/purchase-orders/${id}`).then((r) => r.data),
  create: (data) => api.post('/purchase-orders', data).then((r) => r.data),
  update: (id, data) => api.put(`/purchase-orders/${id}`, data).then((r) => r.data),
  updateStatus: (id, data) => api.patch(`/purchase-orders/${id}/status`, data).then((r) => r.data),
  delete: (id) => api.delete(`/purchase-orders/${id}`),
  generatePdf: (id) => api.post(`/purchase-orders/${id}/generate-pdf`, null, { responseType: 'blob' }),
  getPdf: (id) => api.get(`/purchase-orders/${id}/pdf`, { responseType: 'blob' }),
};

export const grnService = {
  getAll: (params) => api.get('/grns', { params }).then((r) => r.data),
  getById: (id) => api.get(`/grns/${id}`).then((r) => r.data),
  create: (data) => api.post('/grns', data).then((r) => r.data),
  update: (id, data) => api.put(`/grns/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/grns/${id}`),
};

export const powderStockService = {
  getAll: () => api.get('/powder-stock').then((r) => r.data),
  getByPowderColorId: (id) => api.get(`/powder-stock/${id}`).then((r) => r.data),
  getLowStock: () => api.get('/powder-stock/low-stock').then((r) => r.data),
  updateReorderLevel: (powderColorId, data) =>
    api.put(`/powder-stock/${powderColorId}/reorder-level`, data).then((r) => r.data),
};
