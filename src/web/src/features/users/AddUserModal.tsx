import { FC, useCallback, useState } from 'react';
import {
  ChoiceGroup,
  DefaultButton,
  Dialog,
  DialogFooter,
  DialogType,
  Dropdown,
  IChoiceGroupOption,
  IDropdownOption,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Stack,
  TextField,
  Text,
} from '@fluentui/react';
import { isApiError } from '../../services/apiErrors';
import type { Role } from '../../types/roles';
import { assignableRolesForActor } from './roleOptions';
import { useAddUser } from './hooks/useUsers';
import type { AddUserRequest } from './userTypes';

export interface AddUserModalProps {
  isOpen: boolean;
  onDismiss: () => void;
  tenantId: string | null;
  actorRole: Role | null;
  onSuccess?: (result: { kind: string; email?: string }) => void;
}

const modeOptions: IChoiceGroupOption[] = [
  { key: 'existing', text: 'Add existing user (by Entra object ID)' },
  { key: 'invite', text: 'Invite by email' },
];

const AddUserModal: FC<AddUserModalProps> = ({
  isOpen,
  onDismiss,
  tenantId,
  actorRole,
  onSuccess,
}) => {
  const [mode, setMode] = useState<'existing' | 'invite'>('existing');
  const [userId, setUserId] = useState('');
  const [email, setEmail] = useState('');
  const [role, setRole] = useState<Role>('Viewer');
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  const add = useAddUser(tenantId);

  const roleOptions: IDropdownOption[] = assignableRolesForActor(actorRole).map(
    (r) => ({ key: r, text: r })
  );

  const reset = useCallback(() => {
    setMode('existing');
    setUserId('');
    setEmail('');
    setRole('Viewer');
    setError(null);
    setFieldErrors({});
  }, []);

  const handleDismiss = useCallback(() => {
    if (!add.isPending) {
      reset();
      onDismiss();
    }
  }, [add.isPending, onDismiss, reset]);

  const handleSubmit = useCallback(async () => {
    setError(null);
    setFieldErrors({});
    const uid = userId.trim();
    const em = email.trim();
    if (mode === 'existing') {
      if (em.length > 0) {
        setError('Clear the email field when adding by user ID, or switch to invite mode.');
        return;
      }
      if (!uid) {
        setError('User ID is required.');
        return;
      }
    } else {
      if (uid.length > 0) {
        setError('Clear the user ID field when inviting by email, or switch to add-existing mode.');
        return;
      }
      if (!em) {
        setError('Email is required for an invitation.');
        return;
      }
    }

    const body: AddUserRequest =
      mode === 'existing'
        ? { userId: uid, role }
        : { email: em, role };

    try {
      const res = await add.mutateAsync(body);
      reset();
      onDismiss();
      onSuccess?.({
        kind: res.kind,
        email: mode === 'invite' ? em : undefined,
      });
    } catch (e) {
      if (isApiError(e) && e.fieldErrors) {
        const fe: Record<string, string> = {};
        for (const [k, arr] of Object.entries(e.fieldErrors)) {
          if (arr?.[0]) fe[k] = arr[0];
        }
        if (Object.keys(fe).length > 0) setFieldErrors(fe);
      }
      setError(isApiError(e) ? e.message : 'Request failed.');
    }
  }, [add, email, mode, onDismiss, onSuccess, reset, role, userId]);

  return (
    <Dialog
      hidden={!isOpen}
      onDismiss={handleDismiss}
      dialogContentProps={{
        type: DialogType.largeHeader,
        title: 'Add user',
        subText:
          'Provide either an Entra user object ID or an email address — not both.',
      }}
      modalProps={{ isBlocking: true }}
    >
      <Stack tokens={{ childrenGap: 12 }}>
        {error ? (
          <MessageBar messageBarType={MessageBarType.error}>{error}</MessageBar>
        ) : null}
        <ChoiceGroup
          label="Mode"
          options={modeOptions}
          selectedKey={mode}
          onChange={(_, o) => {
            setMode((o?.key as 'existing' | 'invite') ?? 'existing');
            setError(null);
          }}
        />
        {mode === 'existing' ? (
          <TextField
            label="User ID (Entra oid)"
            required
            value={userId}
            onChange={(_, v) => setUserId(v ?? '')}
            errorMessage={fieldErrors.userId}
          />
        ) : (
          <TextField
            label="Email"
            required
            value={email}
            onChange={(_, v) => setEmail(v ?? '')}
            errorMessage={fieldErrors.email}
          />
        )}
        <Dropdown
          label="Role"
          selectedKey={role}
          options={roleOptions}
          onChange={(_, o) => o && setRole(o.key as Role)}
        />
        <Text variant="small">
          Only Platform Admins can assign the Platform Admin role.
        </Text>
      </Stack>
      <DialogFooter>
        <PrimaryButton
          text="Submit"
          onClick={() => void handleSubmit()}
          disabled={add.isPending}
        />
        <DefaultButton
          text="Cancel"
          onClick={handleDismiss}
          disabled={add.isPending}
        />
      </DialogFooter>
    </Dialog>
  );
};

export default AddUserModal;
