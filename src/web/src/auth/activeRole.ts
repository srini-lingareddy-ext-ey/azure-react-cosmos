import type { AuthMeUser } from './authMe';
import { parseRole, type Role } from '../types/roles';

/**
 * Resolves the application role for the active tenant from GET /api/v1/auth/me payload.
 */
export function getActiveRole(
  user: AuthMeUser | null,
  activeTenant: string | null
): Role | null {
  if (!user || !activeTenant) return null;
  const fromMembership = user.tenants?.find((t) => t.tenantId === activeTenant);
  const raw =
    fromMembership?.role ??
    (user.activeTenant === activeTenant ? user.role : undefined) ??
    user.role;
  return parseRole(raw);
}
