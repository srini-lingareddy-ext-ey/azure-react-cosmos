import { FC, useMemo } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { InteractionStatus, InteractionType } from '@azure/msal-browser';
import { useMsal, useMsalAuthentication } from '@azure/msal-react';
import { Spinner, Stack } from '@fluentui/react';
import config from '../config';
import { getLoginRequestScopes } from './msalConfig';
import SessionExpiredPage from './SessionExpiredPage';
import UnprovisionedUserPage from './UnprovisionedUserPage';
import { useAuth } from './useAuth';

const fullPageSpinner = (
  <Stack
    horizontalAlign="center"
    verticalAlign="center"
    styles={{ root: { minHeight: '100vh' } }}
  >
    <Spinner label="Loading…" />
  </Stack>
);

const AuthGuardMsal: FC = () => {
  const location = useLocation();
  const { inProgress } = useMsal();
  const { isLoading, apiUnauthorized, isUnprovisioned, isAuthenticated } =
    useAuth();

  const redirectRequest = useMemo(() => {
    const path = `${location.pathname}${location.search}`;
    const redirectStartPage =
      typeof window !== 'undefined'
        ? `${window.location.origin}${path}`
        : path;
    return {
      scopes: getLoginRequestScopes(),
      redirectStartPage,
    };
  }, [location.pathname, location.search]);

  useMsalAuthentication(InteractionType.Redirect, redirectRequest);

  const msalBlocking =
    inProgress === InteractionStatus.Startup ||
    inProgress === InteractionStatus.Login ||
    inProgress === InteractionStatus.SsoSilent ||
    inProgress === InteractionStatus.HandleRedirect;

  if (isLoading || msalBlocking) {
    return fullPageSpinner;
  }

  if (apiUnauthorized) {
    return <SessionExpiredPage />;
  }

  if (isUnprovisioned) {
    return <UnprovisionedUserPage />;
  }

  if (!isAuthenticated) {
    return fullPageSpinner;
  }

  return <Outlet />;
};

const AuthGuard: FC = () => {
  if (!config.auth.isEnabled) {
    return <Outlet />;
  }

  return <AuthGuardMsal />;
};

export default AuthGuard;
