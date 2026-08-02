import api from './api';

const profileDiagramService = {
  getAll: (params) =>
    api.get('/profile-diagrams', { params }).then((r) => r.data),

  getById: (id) =>
    api.get(`/profile-diagrams/${id}`).then((r) => r.data),

  /**
   * Gallery lookup — exact match on one or more codes.
   * @param {string[]} codes
   */
  getByCodes: (codes) =>
    api
      .get('/profile-diagrams', {
        params: { codes: codes.join(','), mode: 'exact' },
      })
      .then((r) => r.data),

  /**
   * Partial-match autocomplete suggestions (used while typing in Lookup Diagrams).
   * @param {string} term
   * @param {number} [pageSize=30]
   */
  suggest: (term, pageSize = 30) =>
    api
      .get('/profile-diagrams', { params: { search: term, page: 1, pageSize } })
      .then((r) => r.data),

  create: (data) =>
    api.post('/profile-diagrams', data).then((r) => r.data),

  update: (id, data) =>
    api.put(`/profile-diagrams/${id}`, data).then((r) => r.data),

  delete: (id) =>
    api.delete(`/profile-diagrams/${id}`),

  uploadImage: (id, file) => {
    const formData = new FormData();
    formData.append('file', file);
    return api
      .post(`/profile-diagrams/${id}/upload-image`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data);
  },

  /**
   * Download combined PDF for the given codes.
   * Returns a Blob for the caller to trigger a browser download.
   * @param {string[]} codes
   * @returns {Promise<Blob>}
   */
  downloadPdf: (codes) =>
    api
      .get('/profile-diagrams/download-pdf', {
        params: { codes: codes.join(',') },
        responseType: 'blob',
      })
      .then((r) => r.data),
};

export default profileDiagramService;
