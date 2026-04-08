import {
  Configuration,
  LogLevel,
  PublicClientApplication,
} from '@azure/msal-browser';
import config from '../config';

/**
 * MSAL browser configuration from env (VITE_ENTRA_* / VITE_MSAL_* via config).
 * Call only when config.auth.isEnabled.
 */
export function buildMsalConfiguration(): Configuration {
  const origin =
    typeof window !== 'undefined' ? window.location.origin : '';
  const redirectUri = config.auth.redirectUri || origin;
  const postLogout = config.auth.postLogoutRedirectUri || origin;

  return {
    auth: {
      clientId: config.auth.clientId,
      authority: config.auth.authority,
      redirectUri,
      postLogoutRedirectUri: postLogout,
    },
    cache: {
      cacheLocation: 'sessionStorage',
      storeAuthStateInCookie: false,
    },
    system: {
      loggerOptions: {
        logLevel: import.meta.env.DEV ? LogLevel.Warning : LogLevel.Error,
      },
    },
  };
}

export function createMsalInstance(): PublicClientApplication {
  return new PublicClientApplication(buildMsalConfiguration());
}

/** Scopes requested at login (API + OIDC). */
export function getLoginRequestScopes(): string[] {
  const api = config.auth.apiScopes;
  const base = ['openid', 'profile', 'offline_access'] as const;
  if (api.length === 0) {
    return [...base];
  }
  const merged = new Set<string>([...base, ...api]);
  return Array.from(merged);
}

/** Scopes used for silent / API access token. */
export function getTokenRequestScopes(): string[] {
  if (config.auth.apiScopes.length > 0) {
    return [...config.auth.apiScopes];
  }
  return ['openid', 'profile'];
}
