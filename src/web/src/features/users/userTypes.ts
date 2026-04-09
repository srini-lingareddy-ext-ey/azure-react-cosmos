import type { Role } from '../../types/roles';

/** Matches API <see cref="UserStatus" /> JSON. */
export type UserStatusValue = 'Active' | 'Inactive';

export interface UserResponse {
  userId: string;
  displayName?: string | null;
  email?: string | null;
  role: Role;
  status: UserStatusValue;
  lastLoginAt?: string | null;
}

export interface UserRosterResponse {
  items: UserResponse[];
  totalCount: number;
  limit: number;
  offset: number;
}

export interface AddUserRequest {
  userId?: string | null;
  email?: string | null;
  role: Role;
}

export interface AddUserResponse {
  /** "assignment" | "invitation" */
  kind: string;
  id: string;
}

export interface ChangeRoleRequest {
  role: Role;
}
