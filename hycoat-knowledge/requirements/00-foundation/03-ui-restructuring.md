# 00-foundation/03-ui-restructuring

## Feature ID
`00-foundation/03-ui-restructuring`

## Feature Name
UI Project Restructuring

## Dependencies
None on UI side — this is the first UI step. Can run in parallel with API Track A (00/01/02-api tasks).

## Business Context
The existing `hycoat-ui` project is a default Vite + React template with a counter demo. It needs to be restructured into a proper application with routing, state management, API integration, UI component library, and organized folder structure.

## Current State
- Default Vite + React boilerplate (`App.jsx` has counter demo)
- Only dependencies: `react`, `react-dom`
- No routing, no UI framework, no state management
- `src/services/api.js` exists but needs JWT interceptor setup
- `src/components/ProductList.jsx`, `src/hooks/useProducts.js`, `src/pages/Home.jsx` — placeholder files

---

## Tasks

### 1. Install Dependencies
Run in `hycoat-ui/`:
```bash
npm install react-router-dom @tanstack/react-query axios zustand react-hook-form @hookform/resolvers zod @mui/material @mui/icons-material @emotion/react @emotion/styled recharts dayjs react-hot-toast
```

### 2. Install Dev Dependencies
```bash
npm install -D vite-plugin-pwa @types/node
```

### 3. Remove Boilerplate Files
Delete:
- `src/App.css` (will be replaced by MUI theme)
- `src/assets/react.svg`
- `src/components/ProductList.jsx`
- `src/hooks/useProducts.js`
- `src/pages/Home.jsx`
- `public/vite.svg`

### 4. Create Folder Structure
```
src/
├── main.jsx              (update)
├── App.jsx               (rewrite — router setup)
├── theme.js              (NEW — MUI theme)
├── index.css             (update — keep minimal reset only)
├── components/
│   ├── common/           (NEW — shared UI components)
│   └── layout/           (NEW — app shell components)
├── pages/
│   ├── auth/             (NEW)
│   ├── dashboard/        (NEW)
│   ├── masters/          (NEW)
│   ├── sales/            (NEW)
│   ├── material-inward/  (NEW)
│   ├── ppc/              (NEW)
│   ├── production/       (NEW)
│   ├── quality/          (NEW)
│   ├── dispatch/         (NEW)
│   ├── purchase/         (NEW)
│   ├── reports/          (NEW)
│   └── admin/            (NEW)
├── hooks/                (clear existing, will rebuild per feature)
├── services/
│   └── api.js            (update — JWT interceptor)
├── stores/
│   ├── authStore.js      (NEW)
│   └── uiStore.js        (NEW)
└── utils/
    ├── constants.js      (NEW)
    └── formatters.js     (NEW)
```

### 5. Update `index.html`
```html
<!doctype html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <link rel="icon" href="/favicon.ico" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <meta name="theme-color" content="#1565c0" />
    <meta name="description" content="HyCoat Systems - Internal Operations" />
    <link rel="apple-touch-icon" href="/icons/icon-192x192.png" />
    <link rel="manifest" href="/manifest.json" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet" />
    <title>HyCoat ERP</title>
  </head>
  <body>
    <div id="root"></div>
    <script type="module" src="/src/main.jsx"></script>
  </body>
</html>
```

### 6. Create MUI Theme (`src/theme.js`)
```javascript
import { createTheme } from '@mui/material/styles';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1565c0',       // Blue — matches HyCoat branding
      light: '#5e92f3',
      dark: '#003c8f',
    },
    secondary: {
      main: '#f57c00',       // Orange — accent
      light: '#ffad42',
      dark: '#bb4d00',
    },
    background: {
      default: '#f5f5f5',
      paper: '#ffffff',
    },
  },
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
    h4: { fontWeight: 600 },
    h5: { fontWeight: 600 },
    h6: { fontWeight: 600 },
  },
  shape: {
    borderRadius: 8,
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          fontWeight: 500,
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          boxShadow: '0 1px 3px rgba(0,0,0,0.08), 0 1px 2px rgba(0,0,0,0.12)',
        },
      },
    },
  },
});

export default theme;
```

### 7. Update `src/main.jsx`
```jsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { Toaster } from 'react-hot-toast';
import theme from './theme';
import App from './App';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 5 * 60 * 1000, // 5 minutes
    },
  },
});

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <BrowserRouter>
      <QueryClientProvider client={queryClient}>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <App />
          <Toaster position="top-right" />
        </ThemeProvider>
      </QueryClientProvider>
    </BrowserRouter>
  </StrictMode>
);
```

### 8. Rewrite `src/App.jsx`
```jsx
import { Routes, Route, Navigate } from 'react-router-dom';

// Placeholder pages — will be implemented in later features
const LoginPage = () => <div>Login Page (see 02-auth-system)</div>;
const DashboardPage = () => <div>Dashboard (see 09-dashboards)</div>;

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/dashboard" element={<DashboardPage />} />
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
}

export default App;
```

### 9. Update `src/services/api.js` — Axios Instance with JWT Interceptors
```javascript
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7001';

const api = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor — attach JWT token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor — handle 401 and refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const accessToken = localStorage.getItem('accessToken');
        const refreshToken = localStorage.getItem('refreshToken');

        const { data } = await axios.post(`${API_BASE_URL}/api/auth/refresh-token`, {
          accessToken,
          refreshToken,
        });

        localStorage.setItem('accessToken', data.data.accessToken);
        localStorage.setItem('refreshToken', data.data.refreshToken);

        originalRequest.headers.Authorization = `Bearer ${data.data.accessToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
```

### 10. Create Auth Store (`src/stores/authStore.js`)
```javascript
import { create } from 'zustand';

const useAuthStore = create((set) => ({
  user: null,
  isAuthenticated: false,

  setUser: (user) => set({ user, isAuthenticated: !!user }),

  logout: () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    set({ user: null, isAuthenticated: false });
  },

  // Initialize from localStorage on app load
  initialize: () => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const isExpired = payload.exp * 1000 < Date.now();
        if (!isExpired) {
          set({
            user: {
              id: payload.sub,
              email: payload.email,
              fullName: payload.name,
              role: payload.role,
              department: payload.department,
            },
            isAuthenticated: true,
          });
        }
      } catch {
        // Invalid token format — clear
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
      }
    }
  },
}));

export default useAuthStore;
```

### 11. Create UI Store (`src/stores/uiStore.js`)
```javascript
import { create } from 'zustand';

const useUiStore = create((set) => ({
  sidebarOpen: true,
  mobileDrawerOpen: false,

  toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
  toggleMobileDrawer: () => set((state) => ({ mobileDrawerOpen: !state.mobileDrawerOpen })),
  closeMobileDrawer: () => set({ mobileDrawerOpen: false }),
}));

export default useUiStore;
```

### 12. Create Constants (`src/utils/constants.js`)
```javascript
export const DEPARTMENTS = [
  'Sales', 'PPC', 'SCM', 'Production', 'QA', 'Purchase', 'Finance'
];

export const ROLES = ['Admin', 'Leader', 'User'];

export const PROCESS_TYPES = [
  'Powder Coating', 'Anodizing', 'Wood Effect', 'Chromotizing', 'PVDF', 'Mill Finish'
];

export const ORDER_STATUSES = {
  WORK_ORDER: ['Created', 'MaterialAwaited', 'MaterialReceived', 'InProduction', 'QAComplete', 'Dispatched', 'Invoiced', 'Closed'],
  INQUIRY: ['New', 'QuotationSent', 'BOMReceived', 'PISent', 'Confirmed', 'Closed', 'Lost'],
};

export const GST_RATE = 0.18;
export const CGST_RATE = 0.09;
export const SGST_RATE = 0.09;

export const DFT_MIN_MICRON = 60;
export const DFT_MAX_MICRON = 80;

// Area calculation: SFT = (PerimeterMM × LengthMM × Qty) / 92903.04
export const SQ_MM_TO_SQ_FT = 92903.04;
```

### 13. Create Formatters (`src/utils/formatters.js`)
```javascript
import dayjs from 'dayjs';

export const formatDate = (date) => date ? dayjs(date).format('DD/MM/YYYY') : '—';
export const formatDateTime = (date) => date ? dayjs(date).format('DD/MM/YYYY hh:mm A') : '—';
export const formatCurrency = (amount) => amount != null ? `₹${Number(amount).toLocaleString('en-IN', { minimumFractionDigits: 2 })}` : '—';
export const formatNumber = (num, decimals = 2) => num != null ? Number(num).toFixed(decimals) : '—';
export const formatSFT = (sft) => sft != null ? `${Number(sft).toFixed(2)} SFT` : '—';
```

### 14. Update `src/index.css` — Minimal Reset
```css
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

html, body, #root {
  height: 100%;
  width: 100%;
}

body {
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

/* Scrollbar styling */
::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
::-webkit-scrollbar-track {
  background: transparent;
}
::-webkit-scrollbar-thumb {
  background: #bdbdbd;
  border-radius: 3px;
}
::-webkit-scrollbar-thumb:hover {
  background: #9e9e9e;
}
```

### 15. Update `vite.config.js`
```javascript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:7001',
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
```

> **Note:** PWA plugin will be added in `04-pwa-setup.md`

### 16. Create `.env` File
```
VITE_API_URL=https://localhost:7001
VITE_APP_NAME=HyCoat ERP
```

## Files to Create
| File | Purpose |
|---|---|
| `src/theme.js` | MUI theme configuration |
| `src/stores/authStore.js` | Authentication state (Zustand) |
| `src/stores/uiStore.js` | UI state — sidebar, drawers |
| `src/utils/constants.js` | Shared constants — departments, roles, rates |
| `src/utils/formatters.js` | Date, currency, number formatting |
| `.env` | Environment variables |

## Files to Modify
| File | Changes |
|---|---|
| `package.json` | After npm install — all new deps |
| `index.html` | PWA meta tags, font link, title |
| `vite.config.js` | Add path alias, proxy |
| `src/main.jsx` | Providers: Router, QueryClient, Theme, Toaster |
| `src/App.jsx` | Replace boilerplate with route setup |
| `src/index.css` | Minimal reset only |
| `src/services/api.js` | JWT interceptor, refresh flow |

## Files to Delete
| File | Reason |
|---|---|
| `src/App.css` | Replaced by MUI theme |
| `src/assets/react.svg` | Boilerplate |
| `src/components/ProductList.jsx` | Placeholder |
| `src/hooks/useProducts.js` | Placeholder |
| `src/pages/Home.jsx` | Placeholder |
| `public/vite.svg` | Boilerplate |

## Acceptance Criteria
1. `npm run dev` starts the dev server without errors
2. `npm run build` succeeds
3. Landing on `/` redirects to `/dashboard`
4. `/login` route renders (placeholder page is fine)
5. MUI theme is applied — Inter font, blue primary, orange secondary
6. `react-hot-toast` Toaster renders on the page
7. Axios instance at `src/services/api.js` sends Bearer token from localStorage
8. Axios 401 interceptor attempts token refresh and redirects to `/login` on failure
9. Auth store initializes from localStorage token (if present)
10. No console errors related to missing imports or broken modules
11. Path alias `@/` resolves to `src/` (e.g., `import theme from '@/theme'` works)
12. Proxy `/api` routes to `https://localhost:7001` in dev

## Reference
- **README.md (requirements):** "UI Project Structure (Target)" section
- **README.md (requirements):** "Common Coding Conventions — UI Conventions" section
