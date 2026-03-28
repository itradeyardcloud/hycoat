import api from './api';

export const packingListService = {
  getAll: (params) => api.get('/packing-lists', { params }).then((r) => r.data),
  getById: (id) => api.get(`/packing-lists/${id}`).then((r) => r.data),
  getByWorkOrder: (woId) => api.get(`/packing-lists/by-work-order/${woId}`).then((r) => r.data),
  create: (data) => api.post('/packing-lists', data).then((r) => r.data),
  update: (id, data) => api.put(`/packing-lists/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/packing-lists/${id}`),
};

export const deliveryChallanService = {
  getAll: (params) => api.get('/delivery-challans', { params }).then((r) => r.data),
  getById: (id) => api.get(`/delivery-challans/${id}`).then((r) => r.data),
  create: (data) => api.post('/delivery-challans', data).then((r) => r.data),
  update: (id, data) => api.put(`/delivery-challans/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/delivery-challans/${id}`),
  updateStatus: (id, data) => api.patch(`/delivery-challans/${id}/status`, data).then((r) => r.data),
  generatePdf: (id) =>
    api.post(`/delivery-challans/${id}/generate-pdf`, null, { responseType: 'blob' }),
  downloadPdf: (id) =>
    api.get(`/delivery-challans/${id}/pdf`, { responseType: 'blob' }),
  uploadLoadingPhotos: (id, files) => {
    const formData = new FormData();
    files.forEach((file) => formData.append('files', file));
    return api.post(`/delivery-challans/${id}/loading-photos`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }).then((r) => r.data);
  },
};

export const invoiceService = {
  getAll: (params) => api.get('/invoices', { params }).then((r) => r.data),
  getById: (id) => api.get(`/invoices/${id}`).then((r) => r.data),
  getByWorkOrder: (woId) => api.get(`/invoices/by-work-order/${woId}`).then((r) => r.data),
  autoFill: (woId) => api.get(`/invoices/auto-fill/${woId}`).then((r) => r.data),
  create: (data) => api.post('/invoices', data).then((r) => r.data),
  update: (id, data) => api.put(`/invoices/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/invoices/${id}`),
  updateStatus: (id, data) => api.patch(`/invoices/${id}/status`, data).then((r) => r.data),
  generatePdf: (id) =>
    api.post(`/invoices/${id}/generate-pdf`, null, { responseType: 'blob' }),
  downloadPdf: (id) =>
    api.get(`/invoices/${id}/pdf`, { responseType: 'blob' }),
  sendEmail: (id, data) => api.post(`/invoices/${id}/send-email`, data).then((r) => r.data),
};
