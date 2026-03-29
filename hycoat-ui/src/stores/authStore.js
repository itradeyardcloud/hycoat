import { create } from 'zustand';
import {
  getPrimaryAccount,
  isBypassAuthEnabled,
  loginRequest,
  msalInstance,
} from '@/config/msalConfig';

const loginAttemptedKey = 'hycoat.msal.loginAttempted';
let initializeInFlight = false;

const useAuthStore = create((set) => ({
  user: null,
  isAuthenticated: false,
  isInitializing: true,
  authError: null,

  setUser: (user) => set({ user, isAuthenticated: !!user }),

  startLogin: async () => {
    if (isBypassAuthEnabled) {
      return;
    }

    set({ authError: null });
    sessionStorage.setItem(loginAttemptedKey, '1');
    await msalInstance.loginRedirect(loginRequest);
  },

  logout: async () => {
    if (isBypassAuthEnabled) {
      set({ user: null, isAuthenticated: false, isInitializing: false });
      return;
    }

    const account = getPrimaryAccount();
    await msalInstance.logoutRedirect({
      account: account || undefined,
      postLogoutRedirectUri: window.location.origin,
    });
  },

  initialize: async () => {
    if (initializeInFlight) {
      return;
    }

    initializeInFlight = true;

    if (isBypassAuthEnabled) {
      console.warn('>>> AUTH BYPASS IS ENABLED — auto-logged in as Dev Admin <<<');
      set({
        user: {
          id: 'dev-bypass-user',
          email: 'admin@hycoat.dev',
          fullName: 'Dev Admin',
          role: 'Admin',
          department: 'Development',
          groups: ['Dashboard.All'],
        },
        isAuthenticated: true,
        isInitializing: false,
        authError: null,
      });
      initializeInFlight = false;
      return;
    }

    try {
      const redirectResult = await msalInstance.handleRedirectPromise();
      if (redirectResult?.account) {
        msalInstance.setActiveAccount(redirectResult.account);
      }

      const account = getPrimaryAccount();
      if (!account) {
        const redirectError = getRedirectErrorMessage();
        if (redirectError) {
          set({
            user: null,
            isAuthenticated: false,
            isInitializing: false,
            authError: redirectError,
          });
          initializeInFlight = false;
          return;
        }

        const loginAttempted = sessionStorage.getItem(loginAttemptedKey) === '1';
        if (loginAttempted) {
          set({
            user: null,
            isAuthenticated: false,
            isInitializing: false,
            authError: 'Sign-in was not completed. Please try again.',
          });
          initializeInFlight = false;
          return;
        }

        sessionStorage.setItem(loginAttemptedKey, '1');
        await msalInstance.loginRedirect(loginRequest);
        initializeInFlight = false;
        return;
      }

      sessionStorage.removeItem(loginAttemptedKey);

      const user = mapMsalAccountToUser(account);
      set({ user, isAuthenticated: true, isInitializing: false, authError: null });
    } catch (error) {
      console.error('MSAL initialization failed', error);
      set({
        user: null,
        isAuthenticated: false,
        isInitializing: false,
        authError: error?.message || 'Authentication failed. Please sign in again.',
      });
    } finally {
      initializeInFlight = false;
    }
  },
}));

function mapMsalAccountToUser(account) {
  const claims = account?.idTokenClaims || {};
  const roles = normalizeRoles(claims.roles || claims.role);
  const groups = normalizeGroups(claims.groups);

  return {
    id: claims.oid || claims.objectidentifier || account.localAccountId,
    email: claims.preferred_username || claims.email || account.username,
    fullName: claims.name || account.name || account.username,
    role: resolveRole(roles),
    department: claims.department || '',
    groups,
  };
}

function normalizeRoles(roles) {
  if (Array.isArray(roles)) {
    return roles;
  }

  if (typeof roles === 'string' && roles.trim()) {
    return [roles.trim()];
  }

  return [];
}

function resolveRole(roles) {
  if (roles.includes('Admin')) return 'Admin';
  if (roles.includes('Leader')) return 'Leader';
  if (roles.includes('User')) return 'User';
  return 'User';
}

function normalizeGroups(groups) {
  if (Array.isArray(groups)) {
    return groups.filter((g) => typeof g === 'string' && g.trim());
  }

  if (typeof groups === 'string' && groups.trim()) {
    return [groups.trim()];
  }

  return [];
}

function getRedirectErrorMessage() {
  if (typeof window === 'undefined') {
    return null;
  }

  const fromSearch = new URLSearchParams(window.location.search);
  const hash = window.location.hash.startsWith('#')
    ? window.location.hash.slice(1)
    : window.location.hash;
  const fromHash = new URLSearchParams(hash);

  const errorCode = fromSearch.get('error') || fromHash.get('error');
  const errorDescription =
    fromSearch.get('error_description') || fromHash.get('error_description');

  if (!errorCode && !errorDescription) {
    return null;
  }

  if (errorDescription) {
    const decoded = decodeURIComponent(errorDescription).replace(/\+/g, ' ');
    return `Microsoft sign-in failed: ${decoded}`;
  }

  return `Microsoft sign-in failed: ${errorCode}`;
}

export default useAuthStore;
