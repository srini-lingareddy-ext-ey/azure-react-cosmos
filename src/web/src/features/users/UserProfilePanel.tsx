import { FC, useEffect, useMemo, useState } from 'react';
import { getTheme } from '@fluentui/react';
import {
  DefaultButton,
  Dropdown,
  IDropdownOption,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Spinner,
  Stack,
  Text,
} from '@fluentui/react';
import type { Role } from '../../types/roles';
import { assignableRolesForActor } from './roleOptions';
import {
  useDeactivateUserMutation,
  useActivateUserMutation,
  usePatchUserRole,
} from './hooks/useUsers';
import type { UserResponse } from './userTypes';
import { getAuthUserId } from './authUserId';
import { useAuth } from '../../auth/useAuth';

export interface UserProfilePanelProps {
  user: UserResponse | null;
  onDismiss: () => void;
  tenantId: string | null;
  actorRole: Role | null;
}

const UserProfilePanel: FC<UserProfilePanelProps> = ({
  user,
  onDismiss,
  tenantId,
  actorRole,
}) => {
  const { user: me } = useAuth();
  const currentUserId = getAuthUserId(me);
  const [roleDraft, setRoleDraft] = useState<Role | undefined>(undefined);
  const [roleError, setRoleError] = useState<string | null>(null);

  const patchRole = usePatchUserRole(tenantId);
  const activate = useActivateUserMutation(tenantId);
  const deactivate = useDeactivateUserMutation(tenantId);

  const isSelf = Boolean(
    user && currentUserId && user.userId === currentUserId
  );

  useEffect(() => {
    if (!user) {
      setRoleDraft(undefined);
      return;
    }
    setRoleDraft(user.role);
  }, [user]);

  const roleOptions: IDropdownOption[] = useMemo(() => {
    const base = assignableRolesForActor(actorRole);
    const set = new Set(base);
    const merged = [...base];
    if (user && !set.has(user.role)) {
      merged.push(user.role);
    }
    return merged.map((r) => ({ key: r, text: r }));
  }, [actorRole, user]);

  const theme = getTheme();

  if (!user) return null;

  const onSaveRole = async () => {
    if (!tenantId || !roleDraft || roleDraft === user.role) return;
    if (isSelf) return;
    setRoleError(null);
    try {
      await patchRole.mutateAsync({ userId: user.userId, role: roleDraft });
    } catch (e) {
      setRoleError(e instanceof Error ? e.message : 'Failed to update role.');
    }
  };

  const onToggleActive = async () => {
    if (!tenantId) return;
    setRoleError(null);
    try {
      if (user.status === 'Inactive') {
        await activate.mutateAsync(user.userId);
      } else {
        await deactivate.mutateAsync(user.userId);
      }
    } catch (e) {
      setRoleError(e instanceof Error ? e.message : 'Failed to update status.');
    }
  };

  return (
    <>
      <div
        style={{
          position: 'fixed',
          inset: 0,
          background: 'rgba(0,0,0,0.45)',
          zIndex: 100000,
        }}
        onClick={onDismiss}
        onKeyDown={(e) => e.key === 'Escape' && onDismiss()}
        aria-hidden
      />
      <aside
        style={{
          position: 'fixed',
          top: 0,
          right: 0,
          height: '100vh',
          width: 420,
          zIndex: 100001,
          background: theme.palette.neutralLighterAlt,
          boxShadow: theme.effects.elevation64,
          padding: 24,
          overflow: 'auto',
        }}
        aria-label="User profile"
      >
        <Stack tokens={{ childrenGap: 16 }}>
          <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
            <Text variant="xLarge">User</Text>
            <DefaultButton text="Close" onClick={onDismiss} />
          </Stack>

          {roleError ? (
            <MessageBar messageBarType={MessageBarType.error}>
              {roleError}
            </MessageBar>
          ) : null}

          <Text variant="medium">
            <strong>{user.displayName ?? '—'}</strong>
          </Text>
          <Text variant="small">{user.email ?? '—'}</Text>
          <Text variant="small">User ID: {user.userId}</Text>
          <Text variant="small">Status: {user.status}</Text>
          <Text variant="small">
            Last login:{' '}
            {user.lastLoginAt
              ? new Date(user.lastLoginAt).toLocaleString()
              : '—'}
          </Text>

          <Dropdown
            label="Role"
            selectedKey={roleDraft}
            options={roleOptions}
            disabled={isSelf || patchRole.isPending}
            onChange={(_, o) => {
              if (o) setRoleDraft(o.key as Role);
            }}
          />
          {isSelf ? (
            <MessageBar messageBarType={MessageBarType.info}>
              You cannot change your own role in this tenant.
            </MessageBar>
          ) : null}

          <PrimaryButton
            text="Save role"
            onClick={() => void onSaveRole()}
            disabled={
              isSelf ||
              patchRole.isPending ||
              !roleDraft ||
              roleDraft === user.role
            }
          />

          {activate.isPending || deactivate.isPending ? (
            <Spinner label="Updating…" />
          ) : null}

          {user.status === 'Inactive' ? (
            <PrimaryButton
              text="Activate user"
              onClick={() => void onToggleActive()}
              disabled={activate.isPending}
            />
          ) : (
            <DefaultButton
              text="Deactivate user"
              onClick={() => void onToggleActive()}
              disabled={deactivate.isPending}
            />
          )}
        </Stack>
      </aside>
    </>
  );
};

export default UserProfilePanel;
