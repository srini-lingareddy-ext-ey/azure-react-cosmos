import { FC, PropsWithChildren, ReactNode } from 'react';
import config from '../config';
import type { Role } from '../types/roles';
import { rolesMatch } from '../types/roles';
import AccessDeniedView from './AccessDeniedView';
import { useAuth } from './useAuth';

export interface RoleGuardProps extends PropsWithChildren {
  requiredRoles: readonly Role[];
  fallback?: ReactNode;
}

const RoleGuard: FC<RoleGuardProps> = ({
  requiredRoles,
  children,
  fallback = <AccessDeniedView />,
}) => {
  const { activeRole } = useAuth();

  if (!config.auth.isEnabled) {
    return <>{children}</>;
  }

  if (rolesMatch(requiredRoles, activeRole)) {
    return <>{children}</>;
  }

  return <>{fallback}</>;
};

export default RoleGuard;
