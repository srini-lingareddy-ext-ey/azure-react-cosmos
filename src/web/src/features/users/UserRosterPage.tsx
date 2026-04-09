import { FC, useEffect, useMemo, useState } from 'react';
import {
  CheckboxVisibility,
  DetailsList,
  Dropdown,
  IColumn,
  IDropdownOption,
  Link,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  SelectionMode,
  Spinner,
  Stack,
  Text,
} from '@fluentui/react';
import { useAuth } from '../../auth/useAuth';
import { useToast } from '../../components/shared/ToastProvider';
import { ROLES } from '../../types/roles';
import AddUserModal from './AddUserModal';
import UserProfilePanel from './UserProfilePanel';
import {
  useUserRoster,
  USERS_PAGE_SIZE,
  type RoleFilter,
  type StatusFilter,
} from './hooks/useUsers';
import type { UserResponse } from './userTypes';

const roleFilterOptions: IDropdownOption[] = [
  { key: 'all', text: 'All roles' },
  ...ROLES.map((r) => ({ key: r, text: r })),
];

const statusFilterOptions: IDropdownOption[] = [
  { key: 'all', text: 'All statuses' },
  { key: 'Active', text: 'Active' },
  { key: 'Inactive', text: 'Inactive' },
];

function formatLastLogin(iso: string | null | undefined): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleString();
}

const UserRosterPage: FC = () => {
  const { activeTenant, activeRole: actorRole } = useAuth();
  const showToast = useToast();
  const [roleFilter, setRoleFilter] = useState<RoleFilter>('all');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('all');
  const [page, setPage] = useState(0);
  const [selectedUser, setSelectedUser] = useState<UserResponse | null>(null);
  const [addOpen, setAddOpen] = useState(false);

  const { data, isLoading, isError, error, refetch, isFetching } =
    useUserRoster(activeTenant, roleFilter, statusFilter, page);

  useEffect(() => {
    if (!data?.items || !selectedUser) return;
    const next = data.items.find((u) => u.userId === selectedUser.userId);
    if (!next) return;
    setSelectedUser((prev) => {
      if (!prev || prev.userId !== next.userId) return prev;
      if (
        prev.role === next.role &&
        prev.status === next.status &&
        (prev.displayName ?? '') === (next.displayName ?? '') &&
        (prev.email ?? '') === (next.email ?? '') &&
        (prev.lastLoginAt ?? '') === (next.lastLoginAt ?? '')
      ) {
        return prev;
      }
      return next;
    });
  }, [data, selectedUser]);

  const totalPages = useMemo(() => {
    const total = data?.totalCount ?? 0;
    return Math.max(1, Math.ceil(total / USERS_PAGE_SIZE));
  }, [data?.totalCount]);

  useEffect(() => {
    setPage((p) => Math.min(p, Math.max(0, totalPages - 1)));
  }, [totalPages]);

  const safePage = Math.min(page, totalPages - 1);

  const columns: IColumn[] = useMemo(
    () => [
      {
        key: 'displayName',
        name: 'Display name',
        fieldName: 'displayName',
        minWidth: 140,
        onRender: (item: UserResponse) => (
          <span>{item.displayName ?? '—'}</span>
        ),
      },
      {
        key: 'email',
        name: 'Email',
        fieldName: 'email',
        minWidth: 180,
        onRender: (item: UserResponse) => <span>{item.email ?? '—'}</span>,
      },
      { key: 'role', name: 'Role', fieldName: 'role', minWidth: 120 },
      { key: 'status', name: 'Status', fieldName: 'status', minWidth: 90 },
      {
        key: 'lastLoginAt',
        name: 'Last login',
        minWidth: 160,
        onRender: (item: UserResponse) => (
          <span>{formatLastLogin(item.lastLoginAt)}</span>
        ),
      },
    ],
    []
  );

  if (!activeTenant) {
    return (
      <MessageBar messageBarType={MessageBarType.warning}>
        Select an active tenant (via your profile / tenant context) to manage
        users. The API requires <code>X-Tenant-Id</code> to match the tenant.
      </MessageBar>
    );
  }

  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
        <Text variant="xxLarge">Users</Text>
        <PrimaryButton text="Add user" onClick={() => setAddOpen(true)} />
      </Stack>

      <Stack horizontal tokens={{ childrenGap: 16 }} wrap>
        <Dropdown
          label="Role filter"
          selectedKey={roleFilter}
          options={roleFilterOptions}
          styles={{ dropdown: { width: 200 } }}
          onChange={(_, o) => {
            setRoleFilter((o?.key as RoleFilter) ?? 'all');
            setPage(0);
          }}
        />
        <Dropdown
          label="Status filter"
          selectedKey={statusFilter}
          options={statusFilterOptions}
          styles={{ dropdown: { width: 200 } }}
          onChange={(_, o) => {
            setStatusFilter((o?.key as StatusFilter) ?? 'all');
            setPage(0);
          }}
        />
      </Stack>

      {isError ? (
        <MessageBar messageBarType={MessageBarType.error}>
          {(error as Error)?.message ?? 'Failed to load users.'}{' '}
          <Link onClick={() => void refetch()}>Retry</Link>
        </MessageBar>
      ) : null}

      {isLoading || (isFetching && !data) ? (
        <Spinner label="Loading users…" />
      ) : (
        <>
          <DetailsList
            items={data?.items ?? []}
            columns={columns}
            getKey={(item) => item.userId}
            selectionMode={SelectionMode.none}
            checkboxVisibility={CheckboxVisibility.hidden}
            onActiveItemChanged={(item) => {
              if (item) setSelectedUser(item as UserResponse);
            }}
          />
          <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
            <Text variant="small">
              {data?.totalCount === 0
                ? 'No users match this filter.'
                : `Showing ${(data?.offset ?? 0) + 1}–${Math.min(
                    (data?.offset ?? 0) + (data?.items.length ?? 0),
                    data?.totalCount ?? 0
                  )} of ${data?.totalCount ?? 0}`}
            </Text>
            <Stack horizontal tokens={{ childrenGap: 8 }}>
              <PrimaryButton
                text="Previous"
                disabled={safePage <= 0}
                onClick={() => setPage((p) => Math.max(0, p - 1))}
              />
              <Text variant="small">
                Page {safePage + 1} / {totalPages}
              </Text>
              <PrimaryButton
                text="Next"
                disabled={safePage >= totalPages - 1}
                onClick={() =>
                  setPage((p) => Math.min(totalPages - 1, p + 1))
                }
              />
            </Stack>
          </Stack>
        </>
      )}

      <UserProfilePanel
        user={selectedUser}
        onDismiss={() => setSelectedUser(null)}
        tenantId={activeTenant}
        actorRole={actorRole}
      />

      <AddUserModal
        isOpen={addOpen}
        onDismiss={() => setAddOpen(false)}
        tenantId={activeTenant}
        actorRole={actorRole}
        onSuccess={({ kind, email }) => {
          void refetch();
          if (kind === 'invitation' && email) {
            showToast(
              `Invitation sent to ${email}.`,
              MessageBarType.success
            );
          }
        }}
      />
    </Stack>
  );
};

export default UserRosterPage;
