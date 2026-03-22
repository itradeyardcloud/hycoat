import api from './api';

export const customerService = {
  getAll: (params) => api.get('/customers', { params }).then((r) => r.data),
  getById: (id) => api.get(`/customers/${id}`).then((r) => r.data),
  create: (data) => api.post('/customers', data).then((r) => r.data),
  update: (id, data) => api.put(`/customers/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/customers/${id}`),
  lookup: () => api.get('/customers/lookup').then((r) => r.data),
};

export const sectionProfileService = {
  getAll: (params) => api.get('/section-profiles', { params }).then((r) => r.data),
  getById: (id) => api.get(`/section-profiles/${id}`).then((r) => r.data),
  create: (data) => api.post('/section-profiles', data).then((r) => r.data),
  update: (id, data) => api.put(`/section-profiles/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/section-profiles/${id}`),
  lookup: () => api.get('/section-profiles/lookup').then((r) => r.data),
  uploadDrawing: (id, file) => {
    const formData = new FormData();
    formData.append('file', file);
    return api
      .post(`/section-profiles/${id}/upload-drawing`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data);
  },
};

export const powderColorService = {
  getAll: (params) => api.get('/powder-colors', { params }).then((r) => r.data),
  getById: (id) => api.get(`/powder-colors/${id}`).then((r) => r.data),
  create: (data) => api.post('/powder-colors', data).then((r) => r.data),
  update: (id, data) => api.put(`/powder-colors/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/powder-colors/${id}`),
  lookup: () => api.get('/powder-colors/lookup').then((r) => r.data),
};

export const vendorService = {
  getAll: (params) => api.get('/vendors', { params }).then((r) => r.data),
  getById: (id) => api.get(`/vendors/${id}`).then((r) => r.data),
  create: (data) => api.post('/vendors', data).then((r) => r.data),
  update: (id, data) => api.put(`/vendors/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/vendors/${id}`),
  lookup: () => api.get('/vendors/lookup').then((r) => r.data),
};

export const processTypeService = {
  getAll: () => api.get('/process-types').then((r) => r.data),
  getById: (id) => api.get(`/process-types/${id}`).then((r) => r.data),
  create: (data) => api.post('/process-types', data).then((r) => r.data),
  update: (id, data) => api.put(`/process-types/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/process-types/${id}`),
};

export const productionUnitService = {
  getAll: () => api.get('/production-units').then((r) => r.data),
  getById: (id) => api.get(`/production-units/${id}`).then((r) => r.data),
  create: (data) => api.post('/production-units', data).then((r) => r.data),
  update: (id, data) => api.put(`/production-units/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/production-units/${id}`),
};
