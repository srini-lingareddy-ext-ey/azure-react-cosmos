import {
  useMutation,
  useQuery,
  useQueryClient,
  QueryClient,
  type QueryKey,
} from '@tanstack/react-query';
import {
  activateUser,
  addUser,
  deactivateUser,
  getUserRoster,
  patchUserRole,
  type RosterQueryParams,
} from '../userService';
import type {
  AddUserRequest,
  AddUserResponse,
  UserResponse,
  UserRosterResponse,
  UserStatusValue,
} from '../userTypes';
import type { Role } from '../../../types/roles';

export const USERS_PAGE_SIZE = 50;

export type RoleFilter = Role | 'all';
export type StatusFilter = UserStatusValue | 'all';

export function userRosterQueryKey(
  tenantId: string,
  roleFilter: RoleFilter,
  statusFilter: StatusFilter,
  page: number
) {
  return [
    'users',
    tenantId,
    { role: roleFilter, status: statusFilter, page },
  ] as const;
}

function rosterParams(
  roleFilter: RoleFilter,
  statusFilter: StatusFilter,
  page: number
): RosterQueryParams {
  const limit = USERS_PAGE_SIZE;
  const offset = page * limit;
  return {
    limit,
    offset,
    ...(roleFilter !== 'all' ? { role: roleFilter } : {}),
    ...(statusFilter !== 'all' ? { status: statusFilter } : {}),
  };
}

export function useUserRoster(
  tenantId: string | null,
  roleFilter: RoleFilter,
  statusFilter: StatusFilter,
  page: number
) {
  return useQuery({
    queryKey: tenantId
      ? userRosterQueryKey(tenantId, roleFilter, statusFilter, page)
      : ['users', 'none'],
    queryFn: () => getUserRoster(tenantId!, rosterParams(roleFilter, statusFilter, page)),
    enabled: Boolean(tenantId),
  });
}

export function useAddUser(tenantId: string | null) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: AddUserRequest) => {
      if (!tenantId) throw new Error('No tenant selected.');
      return addUser(tenantId, body);
    },
    onSuccess: () => {
      if (!tenantId) return;
      void qc.invalidateQueries({ queryKey: ['users', tenantId] });
    },
  });
}

export function usePatchUserRole(tenantId: string | null) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      userId,
      role,
    }: {
      userId: string;
      role: Role;
    }) => {
      if (!tenantId) throw new Error('No tenant selected.');
      return patchUserRole(tenantId, userId, { role });
    },
    onSuccess: () => {
      if (!tenantId) return;
      void qc.invalidateQueries({ queryKey: ['users', tenantId] });
    },
  });
}

function snapshotUserQueries(
  qc: QueryClient,
  tenantId: string
): [QueryKey, UserRosterResponse | undefined][] {
  return qc.getQueriesData<UserRosterResponse>({
    queryKey: ['users', tenantId],
  });
}

function updateUserInAllRosters(
  qc: QueryClient,
  tenantId: string,
  userId: string,
  patch: Partial<UserResponse>
) {
  const entries = qc.getQueriesData<UserRosterResponse>({
    queryKey: ['users', tenantId],
  });
  entries.forEach(([key, data]) => {
    if (!data?.items) return;
    qc.setQueryData(key, {
      ...data,
      items: data.items.map((u) =>
        u.userId === userId ? { ...u, ...patch } : u
      ),
    });
  });
}

export function useActivateUserMutation(tenantId: string | null) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (userId: string) => {
      if (!tenantId) throw new Error('No tenant selected.');
      return activateUser(tenantId, userId);
    },
    onMutate: async (userId) => {
      if (!tenantId) return;
      await qc.cancelQueries({ queryKey: ['users', tenantId] });
      const previous = snapshotUserQueries(qc, tenantId);
      updateUserInAllRosters(qc, tenantId, userId, { status: 'Active' });
      return { previous };
    },
    onError: (_e, _id, ctx) => {
      if (!tenantId || !ctx?.previous) return;
      ctx.previous.forEach(([key, data]) => {
        qc.setQueryData(key, data);
      });
    },
    onSettled: () => {
      if (!tenantId) return;
      void qc.invalidateQueries({ queryKey: ['users', tenantId] });
    },
  });
}

export function useDeactivateUserMutation(tenantId: string | null) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (userId: string) => {
      if (!tenantId) throw new Error('No tenant selected.');
      return deactivateUser(tenantId, userId);
    },
    onMutate: async (userId) => {
      if (!tenantId) return;
      await qc.cancelQueries({ queryKey: ['users', tenantId] });
      const previous = snapshotUserQueries(qc, tenantId);
      updateUserInAllRosters(qc, tenantId, userId, { status: 'Inactive' });
      return { previous };
    },
    onError: (_e, _id, ctx) => {
      if (!tenantId || !ctx?.previous) return;
      ctx.previous.forEach(([key, data]) => {
        qc.setQueryData(key, data);
      });
    },
    onSettled: () => {
      if (!tenantId) return;
      void qc.invalidateQueries({ queryKey: ['users', tenantId] });
    },
  });
}

export type { AddUserRequest, AddUserResponse };
