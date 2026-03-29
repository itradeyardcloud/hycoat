import api from './api';

export const fileService = {
  upload: ({ file, entityType, entityId, category }) => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('entityType', entityType);
    formData.append('entityId', String(entityId));
    if (category) {
      formData.append('category', category);
    }

    return api.post('/files/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }).then((r) => r.data);
  },
  listByEntity: (entityType, entityId) =>
    api.get(`/files/entity/${entityType}/${entityId}`).then((r) => r.data),
  remove: (id) => api.delete(`/files/${id}`),
  download: (id) => api.get(`/files/${id}`, { responseType: 'blob' }),
};
