/**
 * Typed Vite env accessors (WO-6). Prefer VITE_ENTRA_* / VITE_API_SCOPE; fall back to legacy VITE_MSAL_*.
 */

function trim(value: string | undefined): string {
  return (value ?? '').trim();
}

export function getEntraClientId(): string {
  return trim(import.meta.env.VITE_ENTRA_CLIENT_ID) || trim(import.meta.env.VITE_MSAL_CLIENT_ID);
}

export function getEntraAuthority(): string {
  return (
    trim(import.meta.env.VITE_ENTRA_AUTHORITY) ||
    trim(import.meta.env.VITE_MSAL_AUTHORITY) ||
    'https://login.microsoftonline.com/common'
  );
}

export function getApiBaseUrl(): string {
  return trim(import.meta.env.VITE_API_BASE_URL) || 'http://localhost:3100';
}

export function parseApiScopes(): string[] {
  const single = trim(import.meta.env.VITE_API_SCOPE);
  const legacy = trim(import.meta.env.VITE_MSAL_API_SCOPES);
  const raw = single || legacy;
  if (!raw) return [];
  return raw
    .split(',')
    .map((s) => s.trim())
    .filter(Boolean);
}

export function getMsalRedirectUri(): string {
  return (
    trim(import.meta.env.VITE_MSAL_REDIRECT_URI) ||
    trim(import.meta.env.VITE_ENTRA_REDIRECT_URI)
  );
}

export function getMsalPostLogoutRedirectUri(): string {
  return trim(import.meta.env.VITE_MSAL_POST_LOGOUT_REDIRECT_URI);
}

export function getApplicationInsightsConnectionString(): string {
  return trim(import.meta.env.VITE_APPLICATIONINSIGHTS_CONNECTION_STRING);
}
