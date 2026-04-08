/**
 * Expected shape for GET /api/v1/auth/me (WO-7). Fields optional until API is finalized.
 */
export interface AuthMeTenantRef {
  tenantId: string;
  name?: string;
}

export interface AuthMeUser {
  id?: string;
  displayName?: string;
  email?: string;
  tenants?: AuthMeTenantRef[];
  roles?: string[];
}

export function isAuthMeUser(value: unknown): value is AuthMeUser {
  return typeof value === 'object' && value !== null;
}
