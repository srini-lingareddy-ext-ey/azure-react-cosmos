import {
  createContext,
  FC,
  PropsWithChildren,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useMsal } from '@azure/msal-react';
import { InteractionRequiredAuthError } from '@azure/msal-browser';
import config from '../config';
import {
  apiClient,
  registerApiTokenGetter,
  registerTenantIdGetter,
  resetApiTokenGetter,
  resetTenantIdGetter,
} from '../services/apiClient';
import { getActiveRole } from './activeRole';
import {
  AuthMeUser,
  isAuthMeUser,
  isUserNotProvisionedResponse,
} from './authMe';
import type { Role } from '../types/roles';
import {
  getLoginRequestScopes,
  getTokenRequestScopes,
} from './msalConfig';

const ACTIVE_TENANT_STORAGE_KEY = 'fdi-active-tenant-id';

export interface AuthContextValue {
  user: AuthMeUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  /** True when GET /api/v1/auth/me returned 401 (API rejected the token). */
  apiUnauthorized: boolean;
  isUnprovisioned: boolean;
  activeRole: Role | null;
  activeTenant: string | null;
  setActiveTenant: (tenantId: string | null) => void;
  logout: () => void;
}

const AUTH_DISABLED: AuthContextValue = {
  user: null,
  isAuthenticated: true,
  isLoading: false,
  apiUnauthorized: false,
  isUnprovisioned: false,
  activeRole: null,
  activeTenant: null,
  setActiveTenant: () => {},
  logout: () => {},
};

export const AuthContext = createContext<AuthContextValue | null>(null);

function readStoredTenantId(): string | null {
  if (typeof window === 'undefined') return null;
  const v = localStorage.getItem(ACTIVE_TENANT_STORAGE_KEY);
  return v?.trim() || null;
}

function resolveInitialTenant(
  payload: AuthMeUser | null,
  stored: string | null
): string | null {
  const ids = payload?.tenants?.map((t) => t.tenantId) ?? [];
  if (stored && ids.includes(stored)) return stored;
  if (ids.length > 0) return ids[0] ?? null;
  return null;
}

const AuthContextProviderMsal: FC<PropsWithChildren> = ({ children }) => {
  const { instance, accounts } = useMsal();
  const [user, setUser] = useState<AuthMeUser | null>(null);
  const [meLoading, setMeLoading] = useState(false);
  const [meUnauthorized, setMeUnauthorized] = useState(false);
  const [meUnprovisioned, setMeUnprovisioned] = useState(false);
  const [activeTenant, setActiveTenantState] = useState<string | null>(
    readStoredTenantId
  );

  const activeTenantRef = useRef<string | null>(activeTenant);
  activeTenantRef.current = activeTenant;

  const account =
    instance.getActiveAccount() ?? accounts[0] ?? null;
  const accountKey =
    account?.homeAccountId ?? account?.localAccountId ?? null;

  const setActiveTenant = useCallback((tenantId: string | null) => {
    setActiveTenantState(tenantId);
    if (typeof window === 'undefined') return;
    if (tenantId) {
      localStorage.setItem(ACTIVE_TENANT_STORAGE_KEY, tenantId);
    } else {
      localStorage.removeItem(ACTIVE_TENANT_STORAGE_KEY);
    }
  }, []);

  const logout = useCallback(() => {
    const origin =
      typeof window !== 'undefined' ? window.location.origin : '';
    const postLogout = config.auth.postLogoutRedirectUri || origin;
    setUser(null);
    setMeUnauthorized(false);
    setMeUnprovisioned(false);
    setActiveTenantState(null);
    if (typeof window !== 'undefined') {
      localStorage.removeItem(ACTIVE_TENANT_STORAGE_KEY);
    }
    instance.logoutRedirect({
      postLogoutRedirectUri: postLogout,
    });
  }, [instance]);

  useEffect(() => {
    if (!config.auth.isEnabled) {
      return;
    }
    const first = accounts[0];
    if (first && !instance.getActiveAccount()) {
      instance.setActiveAccount(first);
    }
  }, [instance, accounts]);

  useEffect(() => {
    if (!config.auth.isEnabled) {
      return;
    }

    registerTenantIdGetter(() => activeTenantRef.current);

    registerApiTokenGetter(async () => {
      const acc =
        instance.getActiveAccount() ?? accounts[0] ?? instance.getAllAccounts()[0];
      if (!acc) {
        return null;
      }
      const scopes = getTokenRequestScopes();
      try {
        const result = await instance.acquireTokenSilent({
          scopes,
          account: acc,
        });
        return result.accessToken;
      } catch (e) {
        if (e instanceof InteractionRequiredAuthError) {
          try {
            await instance.loginRedirect({
              scopes: getLoginRequestScopes(),
            });
          } catch {
            /* redirect in flight */
          }
        }
        return null;
      }
    });

    return () => {
      resetApiTokenGetter();
      resetTenantIdGetter();
    };
  }, [instance, accounts]);

  useEffect(() => {
    if (!config.auth.isEnabled) {
      return;
    }

    if (!accountKey) {
      setUser(null);
      setMeUnauthorized(false);
      setMeUnprovisioned(false);
      setMeLoading(false);
      setActiveTenant(null);
      return;
    }

    let cancelled = false;

    const run = async () => {
      setMeLoading(true);
      setMeUnauthorized(false);
      setMeUnprovisioned(false);
      try {
        const res = await apiClient.get<unknown>('/api/v1/auth/me', {
          validateStatus: () => true,
        });
        if (cancelled) return;

        if (res.status === 401) {
          setUser(null);
          setMeUnauthorized(true);
          setMeUnprovisioned(false);
          return;
        }

        if (res.status === 403) {
          setMeUnauthorized(false);
          if (isUserNotProvisionedResponse(res.data)) {
            setUser(null);
            setMeUnprovisioned(true);
            setActiveTenant(null);
            return;
          }
          if (isAuthMeUser(res.data)) {
            setMeUnprovisioned(false);
            setUser(res.data);
            const next = resolveInitialTenant(res.data, readStoredTenantId());
            if (next !== null) {
              setActiveTenant(next);
            }
          } else {
            setMeUnprovisioned(false);
            setUser(null);
          }
          return;
        }

        if (res.status === 200 && isAuthMeUser(res.data)) {
          setUser(res.data);
          setMeUnauthorized(false);
          setMeUnprovisioned(false);
          const next = resolveInitialTenant(res.data, readStoredTenantId());
          setActiveTenant(next);
          return;
        }

        setUser(null);
        setMeUnauthorized(false);
        setMeUnprovisioned(false);
      } catch {
        if (!cancelled) {
          setUser(null);
          setMeUnauthorized(false);
          setMeUnprovisioned(false);
        }
      } finally {
        if (!cancelled) {
          setMeLoading(false);
        }
      }
    };

    void run();

    return () => {
      cancelled = true;
    };
  }, [accountKey, setActiveTenant]);

  const value = useMemo((): AuthContextValue => {
    const isAuthenticated =
      !config.auth.isEnabled ||
      (!!account &&
        !meLoading &&
        !meUnauthorized &&
        !meUnprovisioned &&
        !!user);

    const activeRole = getActiveRole(user, activeTenant);

    return {
      user,
      isAuthenticated,
      isLoading: meLoading,
      apiUnauthorized: meUnauthorized,
      isUnprovisioned: meUnprovisioned,
      activeRole,
      activeTenant,
      setActiveTenant,
      logout,
    };
  }, [
    account,
    activeTenant,
    logout,
    meLoading,
    meUnauthorized,
    meUnprovisioned,
    setActiveTenant,
    user,
  ]);

  return (
    <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
  );
};

export const AuthContextProvider: FC<PropsWithChildren> = ({ children }) => {
  if (!config.auth.isEnabled) {
    return (
      <AuthContext.Provider value={AUTH_DISABLED}>{children}</AuthContext.Provider>
    );
  }

  return <AuthContextProviderMsal>{children}</AuthContextProviderMsal>;
};

export default AuthContextProvider;
