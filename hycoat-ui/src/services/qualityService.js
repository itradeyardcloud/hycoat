import api from './api';

export const inProcessInspectionService = {
  getAll: (params) => api.get('/in-process-inspections', { params }).then((r) => r.data),
  getById: (id) => api.get(`/in-process-inspections/${id}`).then((r) => r.data),
  getByPWO: (pwoId) =>
    api.get(`/in-process-inspections/by-pwo/${pwoId}`).then((r) => r.data),
  getDFTTrend: (pwoId) =>
    api.get(`/in-process-inspections/dft-trend/${pwoId}`).then((r) => r.data),
  create: (data) => api.post('/in-process-inspections', data).then((r) => r.data),
  update: (id, data) => api.put(`/in-process-inspections/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/in-process-inspections/${id}`),
};

export const panelTestService = {
  getAll: (params) => api.get('/panel-tests', { params }).then((r) => r.data),
  getById: (id) => api.get(`/panel-tests/${id}`).then((r) => r.data),
  getByPWO: (pwoId) => api.get(`/panel-tests/by-pwo/${pwoId}`).then((r) => r.data),
  create: (data) => api.post('/panel-tests', data).then((r) => r.data),
  update: (id, data) => api.put(`/panel-tests/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/panel-tests/${id}`),
};

export const finalInspectionService = {
  getAll: (params) => api.get('/final-inspections', { params }).then((r) => r.data),
  getById: (id) => api.get(`/final-inspections/${id}`).then((r) => r.data),
  getByPWO: (pwoId) => api.get(`/final-inspections/by-pwo/${pwoId}`).then((r) => r.data),
  create: (data) => api.post('/final-inspections', data).then((r) => r.data),
  update: (id, data) => api.put(`/final-inspections/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/final-inspections/${id}`),
};

export const testCertificateService = {
  getAll: (params) => api.get('/test-certificates', { params }).then((r) => r.data),
  getById: (id) => api.get(`/test-certificates/${id}`).then((r) => r.data),
  getByWorkOrder: (woId) =>
    api.get(`/test-certificates/by-work-order/${woId}`).then((r) => r.data),
  create: (data) => api.post('/test-certificates', data).then((r) => r.data),
  update: (id, data) => api.put(`/test-certificates/${id}`, data).then((r) => r.data),
  delete: (id) => api.delete(`/test-certificates/${id}`),
  generatePdf: (id) =>
    api.post(`/test-certificates/${id}/generate-pdf`, null, { responseType: 'blob' }),
  downloadPdf: (id) =>
    api.get(`/test-certificates/${id}/pdf`, { responseType: 'blob' }),
};
