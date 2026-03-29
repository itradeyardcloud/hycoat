import axios from 'axios';
import { acquireAccessToken, isBypassAuthEnabled } from '@/config/msalConfig';

const configuredApiUrl = (import.meta.env.VITE_API_URL || '').trim();
const useDevProxy =
  import.meta.env.DEV &&
  (!configuredApiUrl || /localhost|127\.0\.0\.1/i.test(configuredApiUrl));
const API_BASE_URL =
  useDevProxy
    ? ''
    : configuredApiUrl ||
      (import.meta.env.DEV ? 'https://localhost:5001' : 'https://hycoat-dev-api.azurewebsites.net');

const api = axios.create({
  baseURL: useDevProxy ? '/api' : `${API_BASE_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor — attach Azure AD access token from MSAL
api.interceptors.request.use(
  async (config) => {
    if (isBypassAuthEnabled) {
      return config;
    }

    const token = await acquireAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response,
  (error) => Promise.reject(error)
);

export default api;
