/// <reference types="react-scripts" />
/// <reference types="vitest/globals" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;
  readonly VITE_APPLICATIONINSIGHTS_CONNECTION_STRING: string;
  readonly VITE_ENTRA_CLIENT_ID: string;
  readonly VITE_ENTRA_AUTHORITY: string;
  readonly VITE_ENTRA_REDIRECT_URI: string;
  readonly VITE_API_SCOPE: string;
  readonly VITE_MSAL_CLIENT_ID: string;
  readonly VITE_MSAL_AUTHORITY: string;
  readonly VITE_MSAL_REDIRECT_URI: string;
  readonly VITE_MSAL_POST_LOGOUT_REDIRECT_URI: string;
  readonly VITE_MSAL_API_SCOPES: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
