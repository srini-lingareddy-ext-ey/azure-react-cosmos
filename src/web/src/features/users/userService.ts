import { apiClient } from '../../services/apiClient';
import type {
  AddUserRequest,
  AddUserResponse,
  ChangeRoleRequest,
  UserRosterResponse,
  UserStatusValue,
} from './userTypes';
import type { Role } from '../../types/roles';

export interface RosterQueryParams {
  role?: Role;
  status?: UserStatusValue;
  limit: number;
  offset: number;
}

function buildRosterParams(p: RosterQueryParams): Record<string, string | number> {
  const out: Record<string, string | number> = {
    limit: p.limit,
    offset: p.offset,
  };
  if (p.role !== undefined) {
    out.role = p.role;
  }
  if (p.status !== undefined) {
    out.status = p.status;
  }
  return out;
}

export async function getUserRoster(
  tenantId: string,
  params: RosterQueryParams
): Promise<UserRosterResponse> {
  const res = await apiClient.get<UserRosterResponse>(
    `/api/v1/tenants/${tenantId}/users`,
    { params: buildRosterParams(params) }
  );
  return res.data;
}

export async function addUser(
  tenantId: string,
  body: AddUserRequest
): Promise<AddUserResponse> {
  const res = await apiClient.post<AddUserResponse>(
    `/api/v1/tenants/${tenantId}/users`,
    body
  );
  return res.data;
}

export async function patchUserRole(
  tenantId: string,
  userId: string,
  body: ChangeRoleRequest
): Promise<void> {
  await apiClient.patch(
    `/api/v1/tenants/${tenantId}/users/${encodeURIComponent(userId)}/role`,
    body
  );
}

export async function activateUser(
  tenantId: string,
  userId: string
): Promise<void> {
  await apiClient.post(
    `/api/v1/tenants/${tenantId}/users/${encodeURIComponent(userId)}/activate`
  );
}

export async function deactivateUser(
  tenantId: string,
  userId: string
): Promise<void> {
  await apiClient.post(
    `/api/v1/tenants/${tenantId}/users/${encodeURIComponent(userId)}/deactivate`
  );
}
