import { FC, useCallback, useState } from 'react';
import {
  Dialog,
  DialogFooter,
  DialogType,
  PrimaryButton,
  DefaultButton,
  TextField,
  Text,
  MessageBar,
  MessageBarType,
  Stack,
} from '@fluentui/react';
import { isApiError } from '../../services/apiErrors';
import { useCreateTenant } from './hooks/useTenants';
import type { CreateTenantRequest } from './tenantTypes';

export interface CreateTenantModalProps {
  isOpen: boolean;
  onDismiss: () => void;
  onCreated?: (tenantId: string) => void;
}

const CreateTenantModal: FC<CreateTenantModalProps> = ({
  isOpen,
  onDismiss,
  onCreated,
}) => {
  const [name, setName] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [logoUrl, setLogoUrl] = useState('');
  const [backgroundImageUrl, setBackgroundImageUrl] = useState('');
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  const create = useCreateTenant();

  const reset = useCallback(() => {
    setName('');
    setDisplayName('');
    setLogoUrl('');
    setBackgroundImageUrl('');
    setSubmitError(null);
    setFieldErrors({});
  }, []);

  const handleDismiss = useCallback(() => {
    const busy = create.isPending;
    if (!busy) {
      reset();
      onDismiss();
    }
  }, [create.isPending, onDismiss, reset]);

  const handleSubmit = useCallback(async () => {
    setSubmitError(null);
    setFieldErrors({});
    const n = name.trim();
    const dn = displayName.trim();
    if (!n || !dn) {
      setSubmitError('Name and display name are required.');
      return;
    }
    const body: CreateTenantRequest = {
      name: n,
      displayName: dn,
      logoUrl: logoUrl.trim() || undefined,
      backgroundImageUrl: backgroundImageUrl.trim() || undefined,
    };
    try {
      const t = await create.mutateAsync(body);
      reset();
      onDismiss();
      onCreated?.(t.id);
    } catch (e) {
      if (isApiError(e) && e.fieldErrors) {
        const fe: Record<string, string> = {};
        for (const [k, arr] of Object.entries(e.fieldErrors)) {
          if (arr?.[0]) fe[k] = arr[0];
        }
        if (Object.keys(fe).length > 0) setFieldErrors(fe);
      }
      setSubmitError(
        isApiError(e) ? e.message : 'Failed to create tenant. Try again.'
      );
    }
  }, [
    backgroundImageUrl,
    create,
    displayName,
    logoUrl,
    name,
    onCreated,
    onDismiss,
    reset,
  ]);

  return (
    <Dialog
      hidden={!isOpen}
      onDismiss={handleDismiss}
      dialogContentProps={{
        type: DialogType.largeHeader,
        title: 'Create tenant',
        subText:
          'Assign tenant admins and users after creation (User Management).',
      }}
      modalProps={{ isBlocking: true }}
    >
      <Stack tokens={{ childrenGap: 12 }}>
        {submitError ? (
          <MessageBar messageBarType={MessageBarType.error}>
            {submitError}
          </MessageBar>
        ) : null}
        <TextField
          label="Name"
          required
          value={name}
          onChange={(_, v) => setName(v ?? '')}
          errorMessage={fieldErrors.name}
        />
        <TextField
          label="Display name"
          required
          value={displayName}
          onChange={(_, v) => setDisplayName(v ?? '')}
          errorMessage={fieldErrors.displayName}
        />
        <TextField
          label="Logo URL"
          value={logoUrl}
          onChange={(_, v) => setLogoUrl(v ?? '')}
          errorMessage={fieldErrors.logoUrl}
        />
        <TextField
          label="Background image URL"
          value={backgroundImageUrl}
          onChange={(_, v) => setBackgroundImageUrl(v ?? '')}
          errorMessage={fieldErrors.backgroundImageUrl}
        />
        <Text variant="small">
          Initial admin assignment is not part of this API; use User
          Management after the tenant exists.
        </Text>
      </Stack>
      <DialogFooter>
        <PrimaryButton
          text="Create"
          onClick={() => void handleSubmit()}
          disabled={create.isPending}
        />
        <DefaultButton
          text="Cancel"
          onClick={handleDismiss}
          disabled={create.isPending}
        />
      </DialogFooter>
    </Dialog>
  );
};

export default CreateTenantModal;
