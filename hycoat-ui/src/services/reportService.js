import api from './api';

export const reportService = {
  getOrderTracker: (params) => api.get('/reports/order-tracker', { params }).then((r) => r.data),
  exportOrderTracker: (params) => api.get('/reports/order-tracker/export', { params, responseType: 'blob' }),
  getProductionThroughput: (params) => api.get('/reports/production-throughput', { params }).then((r) => r.data),
  exportProductionThroughput: (params) => api.get('/reports/production-throughput/export', { params, responseType: 'blob' }),
  getPowderConsumption: (params) => api.get('/reports/powder-consumption', { params }).then((r) => r.data),
  exportPowderConsumption: (params) => api.get('/reports/powder-consumption/export', { params, responseType: 'blob' }),
  getQualitySummary: (params) => api.get('/reports/quality-summary', { params }).then((r) => r.data),
  getCustomerHistory: (customerId) => api.get(`/reports/customer-history/${customerId}`).then((r) => r.data),
  getDispatchRegister: (params) => api.get('/reports/dispatch-register', { params }).then((r) => r.data),
  exportDispatchRegister: (params) => api.get('/reports/dispatch-register/export', { params, responseType: 'blob' }),
};
