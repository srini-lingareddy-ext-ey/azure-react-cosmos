import { ROLES, type Role } from '../../types/roles';

/** Roles the actor may assign when adding or editing users (WO-14). */
export function assignableRolesForActor(actorRole: Role | null): Role[] {
  if (!actorRole) {
    return [...ROLES];
  }
  if (actorRole === 'PlatformAdmin') {
    return [...ROLES];
  }
  return ROLES.filter((r) => r !== 'PlatformAdmin');
}
