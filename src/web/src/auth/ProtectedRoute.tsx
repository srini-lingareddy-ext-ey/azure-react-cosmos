import { FC } from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useIsAuthenticated, useMsal } from '@azure/msal-react';
import { InteractionStatus } from '@azure/msal-browser';
import { Spinner, Stack } from '@fluentui/react';
import config from '../config';

const ProtectedRoute: FC = () => {
  const location = useLocation();

  if (!config.auth.isEnabled) {
    return <Outlet />;
  }

  return <ProtectedRouteMsal locationPath={location.pathname} />;
};

const ProtectedRouteMsal: FC<{ locationPath: string }> = ({
  locationPath,
}) => {
  const isAuthenticated = useIsAuthenticated();
  const { inProgress } = useMsal();

  // MSAL redirect returns to redirectUri (usually `/`) with the auth response in the
  // URL hash. While inProgress is Startup/HandleRedirect/Login/etc., we must not navigate
  // away — React Router would drop the hash and handleRedirectPromise() would never
  // complete, leaving a blank page or broken session.
  if (!isAuthenticated && inProgress !== InteractionStatus.None) {
    return (
      <Stack
        horizontalAlign="center"
        verticalAlign="center"
        styles={{ root: { minHeight: '50vh' } }}
      >
        <Spinner label="Signing in…" />
      </Stack>
    );
  }

  if (!isAuthenticated) {
    return (
      <Navigate
        to="/login"
        replace
        state={{ from: locationPath }}
      />
    );
  }

  return <Outlet />;
};

export default ProtectedRoute;
