# Frontend authentication (MSAL + Microsoft Entra ID)

The web app uses [@azure/msal-react](https://github.com/AzureAD/microsoft-authentication-library-for-js) and **Microsoft Entra ID** (OpenID Connect) for sign-in. API calls attach an **access token** as `Authorization: Bearer <token>` when authentication is enabled.

## Local development mode (no `VITE_MSAL_CLIENT_ID`)

**This mode is for local development convenience only.** It must not be confused with production or any real security boundary.

When **`VITE_MSAL_CLIENT_ID` is unset or empty**:

- **No Microsoft Entra sign-in** — `MsalProvider` is not used; no login redirect, no tokens, no silent acquisition.
- **No `Authorization` header** — the API client explicitly skips Bearer attachment in this mode (no fake or simulated tokens).
- **No authentication or authorization** — the UI label “Local Dev User” is **not** a signed-in identity; routes stay open only because there is no gate, not because you are trusted.
- **Clear UI** — header tag **AUTH DISABLED** and a banner state that authentication is off.

Use **`VITE_MSAL_CLIENT_ID`** (and API scopes as needed) for environments where real Entra sign-in and API tokens are required.

## Deploying to Azure (azd / CI)

Vite embeds `VITE_*` variables at **build** time. For **`azd deploy`**, set MSAL values in the azd environment so hooks receive them (for example `azd env set VITE_MSAL_CLIENT_ID <spa-client-id>` and optional `VITE_MSAL_API_SCOPES`, …), **or** export the same names in your shell before `azd deploy`. The **`web`** service **`prepackage`** hook in `azure.yaml` writes `API_BASE_URL`, Application Insights, and `VITE_MSAL_*` into a temporary `.env.local` for `npm run build`.

For **GitHub Actions**, set repository **Variables** `VITE_MSAL_CLIENT_ID` (required to enable auth) and any optional `VITE_MSAL_*` names—the deploy job passes them into the build. For **Azure Pipelines**, define the same names as pipeline variables on the deploy task’s `env` block (see `.azdo/pipelines/azure-dev.yml`).

Register your deployed site URL as a **redirect URI** on the Entra SPA registration if you set an explicit `VITE_MSAL_REDIRECT_URI`; otherwise the app defaults to the current origin at runtime.

## Enabling authentication

Set **`VITE_MSAL_CLIENT_ID`** to your Entra **single-page application** client ID.

| Variable | Required | Description |
|----------|----------|-------------|
| `VITE_MSAL_CLIENT_ID` | To enable auth | Application (client) ID of the SPA registration |
| `VITE_MSAL_AUTHORITY` | No | Default: `https://login.microsoftonline.com/common`. Use a tenant URL for single-tenant apps |
| `VITE_MSAL_REDIRECT_URI` | No | Defaults to current origin (must match SPA redirect URIs in Entra) |
| `VITE_MSAL_POST_LOGOUT_REDIRECT_URI` | No | Defaults to current origin |
| `VITE_MSAL_API_SCOPES` | Recommended for API calls | Comma-separated API scopes (e.g. `api://<api-app-id>/.default` or exposed scope name) |

Also set **`VITE_API_BASE_URL`** for the backend (see main README).

## Behavior (MSAL enabled)

1. **MsalProvider** wraps the app after `PublicClientApplication.initialize()`.
2. **Protected routes** (`/`, etc.) redirect unauthenticated users to **`/login`**.
3. **Login** uses `loginRedirect` with scopes: `openid`, `profile`, `offline_access`, plus any `VITE_MSAL_API_SCOPES`.
4. **API client** (`apiClient` and `RestService` axios instances) attach a Bearer token via `acquireTokenSilent`, or `acquireTokenRedirect` if interaction is required. Correlation and error mapping: [api-client.md](./api-client.md).
5. **Sign out** uses `logoutRedirect`.

## Entra app registration checklist

- Register a **Single-page application** platform with redirect URI = your dev/prod origin (e.g. `http://localhost:5173`).
- Expose an API (or use `.default`) and grant the SPA delegated permission to that API if the backend validates JWTs for that audience.
- Align **`VITE_MSAL_API_SCOPES`** with the scopes the API expects.

## Files of interest

| Area | Location |
|------|----------|
| MSAL config / scopes | `src/web/src/auth/msalConfig.ts` |
| Provider + init | `src/web/src/auth/MsalAppProvider.tsx` |
| Token → axios | `src/web/src/auth/AuthTokenBridge.tsx`, `src/web/src/services/apiClient.ts` |
| Route guard | `src/web/src/auth/ProtectedRoute.tsx` |
| Login page | `src/web/src/features/auth/LoginPage.tsx` |
| Local-dev banner | `src/web/src/components/shared/LocalDevAuthBanner.tsx` |
| App config | `src/web/src/config/index.ts` |

Backend JWT validation is separate; see backend auth work orders and API Entra configuration.
