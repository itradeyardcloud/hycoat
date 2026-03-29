# hycoat-ui

Frontend app for HyCoat ERP built with React + Vite.

## Local development

1. Install dependencies:
	- `npm ci`
2. Start dev server:
	- `npm run dev`

## Production build

1. Build:
	- `npm run build`
2. Preview build locally:
	- `npm run preview`

## Environment variables

Create `.env` from `.env.example`.

- `VITE_API_URL`: backend API base URL
- `VITE_APP_NAME`: app display name
- `VITE_BYPASS_AUTH`: auth bypass switch for local/dev-only flows

For Azure Dev deployment, the expected API URL is:

- `https://hycoat-dev-api.azurewebsites.net`

## Azure App Service deployment (GitHub Actions)

Workflow file:

- `.github/workflows/deploy-hycoat-ui-azure.yml`

Deployment flow:

1. Triggered on push to `main` when files under `hycoat-ui/` change.
2. Runs `npm ci` and `npm run build` in `hycoat-ui`.
3. Copies `hycoat-ui/web.config` into `dist/` for SPA routing on IIS.
4. Deploys `dist/` to Azure Web App using `azure/webapps-deploy`.

### Required GitHub settings

Repository secret:

- `AZURE_WEBAPP_PUBLISH_PROFILE_HYCOAT_DEV_UI`: Publish profile XML from Azure Web App.

Optional repository variable:

- `HYCOAT_UI_VITE_API_URL`: Overrides build-time API URL.

If `HYCOAT_UI_VITE_API_URL` is not set, workflow defaults to:

- `https://hycoat-dev-api.azurewebsites.net`

### Target Azure resources

- Web App name expected by workflow: `hycoat-dev-ui-win`
- Resource group: `hycoat-dev-si-rg`

Update `AZURE_WEBAPP_NAME` in workflow if your UI web app uses a different name.
