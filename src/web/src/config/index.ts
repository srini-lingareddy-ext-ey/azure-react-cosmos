/// <reference types="vite/client" />

import {
  getApiBaseUrl,
  getApplicationInsightsConnectionString,
  getEntraAuthority,
  getEntraClientId,
  getMsalPostLogoutRedirectUri,
  getMsalRedirectUri,
  parseApiScopes,
} from './env';

export interface ApiConfig {
  baseUrl: string;
}

export interface ObservabilityConfig {
  connectionString: string;
}

/** Entra ID / MSAL settings (browser-safe; client ID is public). */
export interface AuthConfig {
  /** When false, MSAL is not loaded; routes are open and no Bearer token is sent. */
  isEnabled: boolean;
  clientId: string;
  authority: string;
  redirectUri: string;
  postLogoutRedirectUri: string;
  /** Scopes for the backend API (e.g. api://{api-app-id}/access_as_user). */
  apiScopes: string[];
}

export interface AppConfig {
  api: ApiConfig;
  observability: ObservabilityConfig;
  auth: AuthConfig;
}

const clientId = getEntraClientId();

const config: AppConfig = {
  api: {
    baseUrl: getApiBaseUrl(),
  },
  observability: {
    connectionString: getApplicationInsightsConnectionString(),
  },
  auth: {
    isEnabled: Boolean(clientId),
    clientId,
    authority: getEntraAuthority(),
    redirectUri: getMsalRedirectUri(),
    postLogoutRedirectUri: getMsalPostLogoutRedirectUri(),
    apiScopes: parseApiScopes(),
  },
};

export default config;
