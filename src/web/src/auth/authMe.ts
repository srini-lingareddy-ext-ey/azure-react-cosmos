/**
 * Shape for GET /api/v1/auth/me success body (see UserProfileResponse in the API).
 */
export interface AuthMeTenantRef {
  tenantId: string;
  tenantName?: string;
  /** Serialized <see cref="UserRole" /> name, e.g. <c>Admin</c>. */
  role?: string;
}

export interface AuthMeUser {
  id?: string;
  userId?: string;
  displayName?: string;
  email?: string;
  /** Preferred active tenant from the API when <c>X-Tenant-Id</c> is set. */
  activeTenant?: string | null;
  /** Role for the resolved active tenant. */
  role?: string | null;
  tenants?: AuthMeTenantRef[];
}

/**
 * Accepts only payloads that look like GET /api/v1/auth/me success body (not error envelopes or arbitrary objects).
 */
export function isAuthMeUser(value: unknown): value is AuthMeUser {
  if (typeof value !== 'object' || value === null) return false;
  const o = value as Record<string, unknown>;
  if ('errorCode' in o && typeof o.errorCode === 'string') {
    return false;
  }
  const hasUserId = typeof o.userId === 'string' && o.userId.length > 0;
  const hasId = typeof o.id === 'string' && o.id.length > 0;
  if (hasUserId || hasId) {
    return true;
  }
  if (Array.isArray(o.tenants) && o.tenants.length > 0) {
    return o.tenants.every((item) => {
      if (typeof item !== 'object' || item === null) return false;
      const t = item as Record<string, unknown>;
      return typeof t.tenantId === 'string' && t.tenantId.length > 0;
    });
  }
  return false;
}

export function isUserNotProvisionedResponse(value: unknown): boolean {
  if (typeof value !== 'object' || value === null) return false;
  const o = value as { errorCode?: string };
  return o.errorCode === 'USER_NOT_PROVISIONED';
}
