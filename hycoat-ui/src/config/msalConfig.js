import { InteractionRequiredAuthError, PublicClientApplication } from '@azure/msal-browser';

const tenantId = import.meta.env.VITE_AZURE_AD_TENANT_ID || '100c9786-73ec-4f86-922e-236173eb62e0';
const clientId = import.meta.env.VITE_AZURE_AD_CLIENT_ID || 'a1be3ce9-8fdb-4243-9b08-ba745a7425bf';
const apiScope =
  import.meta.env.VITE_AZURE_AD_SCOPE ||
  'api://a1be3ce9-8fdb-4243-9b08-ba745a7425bf/access_as_user';

export const isBypassAuthEnabled = import.meta.env.VITE_BYPASS_AUTH === 'true';

const runtimeOrigin =
  typeof window !== 'undefined' ? window.location.origin : 'http://localhost:5173';
const redirectUri = import.meta.env.VITE_AZURE_AD_REDIRECT_URI || runtimeOrigin;
const postLogoutRedirectUri =
  import.meta.env.VITE_AZURE_AD_POST_LOGOUT_REDIRECT_URI || runtimeOrigin;

const msalConfig = {
  auth: {
    clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri,
    postLogoutRedirectUri,
    navigateToLoginRequestUrl: false,
  },
  cache: {
    cacheLocation: 'localStorage',
    temporaryCacheLocation: 'localStorage',
    storeAuthStateInCookie: false,
  },
};

export const loginRequest = {
  scopes: ['openid', 'profile', 'offline_access'],
};

const tokenRequest = {
  scopes: [apiScope],
};

export const msalInstance = new PublicClientApplication(msalConfig);

let redirectInProgress = false;

export function getPrimaryAccount() {
  const activeAccount = msalInstance.getActiveAccount();
  if (activeAccount) {
    return activeAccount;
  }

  const accounts = msalInstance.getAllAccounts();
  if (accounts.length > 0) {
    msalInstance.setActiveAccount(accounts[0]);
    return accounts[0];
  }

  return null;
}

export async function acquireAccessToken() {
  if (isBypassAuthEnabled) {
    return null;
  }

  const account = getPrimaryAccount();
  if (!account) {
    if (!redirectInProgress) {
      redirectInProgress = true;
      await msalInstance.loginRedirect(loginRequest);
    }
    return null;
  }

  try {
    const response = await msalInstance.acquireTokenSilent({
      ...tokenRequest,
      account,
    });
    return response.accessToken;
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError && !redirectInProgress) {
      redirectInProgress = true;
      await msalInstance.acquireTokenRedirect({
        ...tokenRequest,
        account,
      });
      return null;
    }

    throw error;
  }
}
