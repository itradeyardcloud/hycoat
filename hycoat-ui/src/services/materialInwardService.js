import api from './api';

// Work Order lookup (used by MI form — no dedicated WO module yet)
export const workOrderService = {
  lookup: (params) => api.get('/material-inwards/wo-lookup', { params }).then((r) => r.data),
};

export const materialInwardService = {
  getAll: (params) => api.get('/material-inwards', { params }).then((r) => r.data),
  getById: (id) => api.get(`/material-inwards/${id}`).then((r) => r.data),
  create: (data) => api.post('/material-inwards', data).then((r) => r.data),
  update: (id, data) => api.put(`/material-inwards/${id}`, data).then((r) => r.data),
  updateStatus: (id, data) => api.patch(`/material-inwards/${id}/status`, data).then((r) => r.data),
  delete: (id) => api.delete(`/material-inwards/${id}`),
  lookup: (params) => api.get('/material-inwards/lookup', { params }).then((r) => r.data),
  uploadPhotos: (id, files) => {
    const formData = new FormData();
    files.forEach((file) => formData.append('files', file));
    return api
      .post(`/material-inwards/${id}/photos`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data);
  },
  getPhotos: (id) => api.get(`/material-inwards/${id}/photos`).then((r) => r.data),
  deletePhoto: (id, photoId) => api.delete(`/material-inwards/${id}/photos/${photoId}`),
};

export const incomingInspectionService = {
  getAll: (params) => api.get('/incoming-inspections', { params }).then((r) => r.data),
  getById: (id) => api.get(`/incoming-inspections/${id}`).then((r) => r.data),
  create: (data) => api.post('/incoming-inspections', data).then((r) => r.data),
  update: (id, data) => api.put(`/incoming-inspections/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/incoming-inspections/${id}`),
  uploadPhotos: (id, files) => {
    const formData = new FormData();
    files.forEach((file) => formData.append('files', file));
    return api
      .post(`/incoming-inspections/${id}/photos`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data);
  },
  getPhotos: (id) => api.get(`/incoming-inspections/${id}/photos`).then((r) => r.data),
};
