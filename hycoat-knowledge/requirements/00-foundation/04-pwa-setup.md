# 00-foundation/04-pwa-setup

## Feature ID
`00-foundation/04-pwa-setup`

## Feature Name
Progressive Web App (PWA) Configuration

## Dependencies
- `00-foundation/03-ui-restructuring` — MUI theme, project structure, vite config must exist

## Business Context
HyCoat Systems' production floor operators and QA inspectors use tablets/phones. The app must work as a PWA so it can be installed on home screens, load quickly on slow factory WiFi, and provide basic offline capabilities for form entry during network drops.

---

## Tasks

### 1. Install vite-plugin-pwa
Already installed in `03-ui-restructuring` as dev dependency. Verify:
```bash
npm ls vite-plugin-pwa
```

### 2. Create PWA Icons
Place icons in `public/icons/`:
```
public/
├── favicon.ico              (32×32)
├── icons/
│   ├── icon-72x72.png
│   ├── icon-96x96.png
│   ├── icon-128x128.png
│   ├── icon-144x144.png
│   ├── icon-152x152.png
│   ├── icon-192x192.png
│   ├── icon-384x384.png
│   └── icon-512x512.png
└── apple-touch-icon.png     (180×180)
```
Use a simple blue (#1565c0) background with white "HC" text or HyCoat logo.

### 3. Update `vite.config.js` — Add PWA Plugin
```javascript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { VitePWA } from 'vite-plugin-pwa';
import path from 'path';

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.ico', 'apple-touch-icon.png', 'icons/*.png'],
      manifest: {
        name: 'HyCoat ERP',
        short_name: 'HyCoat',
        description: 'HyCoat Systems - Internal Operations Management',
        theme_color: '#1565c0',
        background_color: '#ffffff',
        display: 'standalone',
        orientation: 'any',
        scope: '/',
        start_url: '/',
        icons: [
          { src: 'icons/icon-72x72.png', sizes: '72x72', type: 'image/png' },
          { src: 'icons/icon-96x96.png', sizes: '96x96', type: 'image/png' },
          { src: 'icons/icon-128x128.png', sizes: '128x128', type: 'image/png' },
          { src: 'icons/icon-144x144.png', sizes: '144x144', type: 'image/png', purpose: 'any' },
          { src: 'icons/icon-152x152.png', sizes: '152x152', type: 'image/png' },
          { src: 'icons/icon-192x192.png', sizes: '192x192', type: 'image/png' },
          { src: 'icons/icon-384x384.png', sizes: '384x384', type: 'image/png' },
          { src: 'icons/icon-512x512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' },
        ],
      },
      workbox: {
        globPatterns: ['**/*.{js,css,html,ico,png,svg,woff2}'],
        runtimeCaching: [
          {
            urlPattern: /^https:\/\/fonts\.googleapis\.com\/.*/i,
            handler: 'CacheFirst',
            options: {
              cacheName: 'google-fonts-cache',
              expiration: { maxEntries: 10, maxAgeSeconds: 60 * 60 * 24 * 365 },
              cacheableResponse: { statuses: [0, 200] },
            },
          },
          {
            urlPattern: /^https:\/\/fonts\.gstatic\.com\/.*/i,
            handler: 'CacheFirst',
            options: {
              cacheName: 'gstatic-fonts-cache',
              expiration: { maxEntries: 10, maxAgeSeconds: 60 * 60 * 24 * 365 },
              cacheableResponse: { statuses: [0, 200] },
            },
          },
          {
            urlPattern: /\/api\/.*$/i,
            handler: 'NetworkFirst',
            options: {
              cacheName: 'api-cache',
              expiration: { maxEntries: 100, maxAgeSeconds: 60 * 60 },
              networkTimeoutSeconds: 10,
              cacheableResponse: { statuses: [0, 200] },
            },
          },
        ],
      },
    }),
  ],
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

### 4. Add PWA Registration in `src/main.jsx`
After the existing imports, add:
```jsx
import { registerSW } from 'virtual:pwa-register';

const updateSW = registerSW({
  onNeedRefresh() {
    if (confirm('New version available. Reload?')) {
      updateSW(true);
    }
  },
  onOfflineReady() {
    console.log('App ready to work offline');
  },
});
```

### 5. Create Offline Fallback Page (`public/offline.html`)
```html
<!doctype html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>HyCoat ERP — Offline</title>
    <style>
      body { font-family: 'Inter', sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; background: #f5f5f5; color: #333; }
      .container { text-align: center; padding: 2rem; }
      h1 { color: #1565c0; }
      p { color: #666; margin-top: 1rem; }
    </style>
  </head>
  <body>
    <div class="container">
      <h1>You're Offline</h1>
      <p>Please check your network connection and try again.</p>
    </div>
  </body>
</html>
```

### 6. Update `index.html` Meta Tags
Ensure these are present (from `03-ui-restructuring`):
```html
<meta name="theme-color" content="#1565c0" />
<meta name="apple-mobile-web-app-capable" content="yes" />
<meta name="apple-mobile-web-app-status-bar-style" content="black-translucent" />
<link rel="apple-touch-icon" href="/apple-touch-icon.png" />
<link rel="manifest" href="/manifest.json" />
```

> **Note:** `vite-plugin-pwa` auto-generates `/manifest.json` from the config. The `<link rel="manifest">` tag is injected automatically.

---

## Files to Create
| File | Purpose |
|---|---|
| `public/icons/*.png` | PWA icons (8 sizes) |
| `public/apple-touch-icon.png` | iOS home screen icon |
| `public/favicon.ico` | Browser tab icon |
| `public/offline.html` | Offline fallback page |

## Files to Modify
| File | Changes |
|---|---|
| `vite.config.js` | Add VitePWA plugin with manifest + workbox config |
| `src/main.jsx` | Add `registerSW` from virtual:pwa-register |
| `index.html` | Add apple-mobile-web-app meta tags |

## Caching Strategy
| Resource | Strategy | Reason |
|---|---|---|
| App shell (JS/CSS/HTML) | **Precache** | Always available offline |
| Google Fonts | **CacheFirst** | Rarely change, OK to serve stale |
| API responses | **NetworkFirst** | Fresh data preferred; fall back to cache during outage |
| Static images/icons | **Precache** | Part of app shell |

## Acceptance Criteria
1. `npm run build` generates `dist/sw.js` and `dist/manifest.webmanifest`
2. Lighthouse PWA audit passes (installable, offline capable)
3. App can be installed on Chrome (desktop + mobile) via "Add to Home Screen"
4. App can be installed on iOS Safari via "Add to Home Screen"
5. After install, app opens in standalone mode (no browser chrome)
6. When new version is deployed, user gets "New version available" prompt
7. Static assets load even when offline
8. API calls fall back to cached responses when offline

## Reference
- **README.md (requirements):** PWA requirements section
- **03-ui-restructuring.md:** Vite config base, index.html meta tags
