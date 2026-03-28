import api from './api';

export const pretreatmentLogService = {
  getAll: (params) => api.get('/pretreatment-logs', { params }).then((r) => r.data),
  getById: (id) => api.get(`/pretreatment-logs/${id}`).then((r) => r.data),
  getByPWO: (pwoId) => api.get(`/pretreatment-logs/by-pwo/${pwoId}`).then((r) => r.data),
  create: (data) => api.post('/pretreatment-logs', data).then((r) => r.data),
  update: (id, data) => api.put(`/pretreatment-logs/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/pretreatment-logs/${id}`),
  addTankReadings: (id, readings) =>
    api.post(`/pretreatment-logs/${id}/tank-readings`, readings).then((r) => r.data),
};

export const productionLogService = {
  getAll: (params) => api.get('/production-logs', { params }).then((r) => r.data),
  getById: (id) => api.get(`/production-logs/${id}`).then((r) => r.data),
  getByPWO: (pwoId) => api.get(`/production-logs/by-pwo/${pwoId}`).then((r) => r.data),
  create: (data) => api.post('/production-logs', data).then((r) => r.data),
  update: (id, data) => api.put(`/production-logs/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/production-logs/${id}`),
  uploadPhoto: (id, file, description) => {
    const formData = new FormData();
    formData.append('file', file);
    if (description) formData.append('description', description);
    return api
      .post(`/production-logs/${id}/photos`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data);
  },
  deletePhoto: (id, photoId) => api.delete(`/production-logs/${id}/photos/${photoId}`),
};
