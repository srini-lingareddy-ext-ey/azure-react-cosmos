export const ROLES = [
  'Viewer',
  'Operator',
  'Admin',
  'ComplianceOfficer',
  'PlatformAdmin',
] as const;

export type Role = (typeof ROLES)[number];

export function parseRole(value: unknown): Role | null {
  if (typeof value !== 'string') return null;
  return (ROLES as readonly string[]).includes(value) ? (value as Role) : null;
}

export function rolesMatch(
  required: readonly Role[],
  actual: Role | null
): boolean {
  if (!actual) return false;
  return required.includes(actual);
}
