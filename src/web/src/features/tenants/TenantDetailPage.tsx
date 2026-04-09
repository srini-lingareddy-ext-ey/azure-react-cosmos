import { FC, useCallback, useEffect, useMemo, useState } from 'react';
import { Link as RouterLink, useParams } from 'react-router-dom';
import {
  DefaultButton,
  Dialog,
  DialogFooter,
  DialogType,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Spinner,
  Stack,
  Text,
  TextField,
} from '@fluentui/react';
import { useToast } from '../../components/shared/ToastProvider';
import {
  useActivateTenant,
  useDeactivateTenant,
  useTenant,
  useUpdateTenantConfig,
} from './hooks/useTenants';
import { healthWeightsSumMessage } from './tenantConfigValidation';
import type {
  HealthStatusThresholds,
  TenantResponse,
  UpdateTenantConfigRequest,
} from './tenantTypes';

type WeightRow = { key: string; value: string };

function tenantToRows(t: TenantResponse | undefined): WeightRow[] {
  const w = t?.config?.healthScoreWeights;
  if (!w || Object.keys(w).length === 0) {
    return [{ key: 'composite', value: '100' }];
  }
  return Object.entries(w).map(([key, value]) => ({
    key,
    value: String(value),
  }));
}

function rowsToWeights(rows: WeightRow[]): Record<string, number> {
  const out: Record<string, number> = {};
  for (const r of rows) {
    const k = r.key.trim();
    if (!k) continue;
    const n = Number.parseFloat(r.value);
    if (!Number.isFinite(n)) continue;
    out[k] = n;
  }
  return out;
}

function thresholdsFromTenant(
  t: TenantResponse | undefined
): HealthStatusThresholds {
  return {
    healthyMin: t?.config?.healthStatusThresholds?.healthyMin ?? undefined,
    warningMin: t?.config?.healthStatusThresholds?.warningMin ?? undefined,
    criticalBelow: t?.config?.healthStatusThresholds?.criticalBelow ?? undefined,
  };
}

function buildThresholdsPatch(
  t: HealthStatusThresholds
): HealthStatusThresholds | undefined {
  const out: HealthStatusThresholds = {};
  if (typeof t.healthyMin === 'number' && Number.isFinite(t.healthyMin)) {
    out.healthyMin = t.healthyMin;
  }
  if (typeof t.warningMin === 'number' && Number.isFinite(t.warningMin)) {
    out.warningMin = t.warningMin;
  }
  if (typeof t.criticalBelow === 'number' && Number.isFinite(t.criticalBelow)) {
    out.criticalBelow = t.criticalBelow;
  }
  return Object.keys(out).length > 0 ? out : undefined;
}

const TenantDetailPage: FC = () => {
  const { id } = useParams<{ id: string }>();
  const showToast = useToast();
  const { data: tenant, isLoading, isError, error } = useTenant(id);

  const [weightRows, setWeightRows] = useState<WeightRow[]>([]);
  const [thresholds, setThresholds] = useState<HealthStatusThresholds>({});
  const [localWeightError, setLocalWeightError] = useState<string | null>(null);
  const [confirmDeactivate, setConfirmDeactivate] = useState(false);

  // Sync form from server only when identity or version changes — avoids wiping edits on refetch with identical data.
  useEffect(() => {
    if (!tenant) return;
    setWeightRows(tenantToRows(tenant));
    setThresholds(thresholdsFromTenant(tenant));
    setLocalWeightError(null);
    // Intentionally omit `tenant`: we only re-sync when id or updatedAt changes, not on every query reference update.
    // eslint-disable-next-line react-hooks/exhaustive-deps -- stable server version gate
  }, [tenant?.id, tenant?.updatedAt]);

  useEffect(() => {
    setLocalWeightError(null);
  }, [weightRows]);

  const updateConfig = useUpdateTenantConfig(id ?? '');
  const activate = useActivateTenant(id ?? '');
  const deactivate = useDeactivateTenant(id ?? '');

  const weightsRecord = useMemo(
    () => rowsToWeights(weightRows),
    [weightRows]
  );

  const onSaveConfig = useCallback(async () => {
    if (!id) return;
    if (weightRows.length === 0) {
      setLocalWeightError('Add at least one health score weight row.');
      return;
    }
    if (Object.keys(weightsRecord).length === 0) {
      setLocalWeightError(
        'Each weight row needs a non-empty key and a numeric value.'
      );
      return;
    }
    const msg = healthWeightsSumMessage(weightsRecord);
    setLocalWeightError(msg);
    if (msg) return;
    const th = buildThresholdsPatch(thresholds);
    const body: UpdateTenantConfigRequest = {
      healthScoreWeights: weightsRecord,
      ...(th ? { healthStatusThresholds: th } : {}),
    };
    try {
      await updateConfig.mutateAsync(body);
      showToast('Configuration saved.', MessageBarType.success);
    } catch (e) {
      showToast(
        e instanceof Error ? e.message : 'Failed to save configuration.',
        MessageBarType.error
      );
    }
  }, [
    id,
    showToast,
    thresholds,
    updateConfig,
    weightRows.length,
    weightsRecord,
  ]);

  const onActivate = useCallback(async () => {
    if (!id) return;
    try {
      await activate.mutateAsync();
      showToast('Tenant activated.', MessageBarType.success);
    } catch (e) {
      showToast(
        e instanceof Error ? e.message : 'Activation failed.',
        MessageBarType.error
      );
    }
  }, [activate, id, showToast]);

  const onDeactivate = useCallback(async () => {
    if (!id) return;
    try {
      await deactivate.mutateAsync();
      setConfirmDeactivate(false);
      showToast('Tenant deactivated.', MessageBarType.success);
    } catch (e) {
      showToast(
        e instanceof Error ? e.message : 'Deactivation failed.',
        MessageBarType.error
      );
    }
  }, [deactivate, id, showToast]);

  if (!id) {
    return <Text>Missing tenant id.</Text>;
  }

  if (isLoading) {
    return <Spinner label="Loading tenant…" />;
  }

  if (isError || !tenant) {
    return (
      <MessageBar messageBarType={MessageBarType.error}>
        {(error as Error)?.message ?? 'Tenant not found.'}{' '}
        <RouterLink to="/admin/tenants">Back to list</RouterLink>
      </MessageBar>
    );
  }

  const sumMsg = healthWeightsSumMessage(weightsRecord);

  return (
    <Stack tokens={{ childrenGap: 20 }}>
      <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
        <Stack tokens={{ childrenGap: 4 }}>
          <RouterLink to="/admin/tenants">← Tenants</RouterLink>
          <Text variant="xxLarge">{tenant.displayName}</Text>
          <Text variant="medium">
            {tenant.name} · Status: {tenant.status}
          </Text>
        </Stack>
        <Stack horizontal tokens={{ childrenGap: 8 }}>
          {tenant.status === 'Inactive' ? (
            <PrimaryButton
              text="Activate"
              onClick={() => void onActivate()}
              disabled={activate.isPending}
            />
          ) : (
            <DefaultButton
              text="Deactivate"
              onClick={() => setConfirmDeactivate(true)}
              disabled={deactivate.isPending}
            />
          )}
        </Stack>
      </Stack>

      <Stack tokens={{ childrenGap: 8 }}>
        <Text variant="large">Branding (read-only)</Text>
        <Text variant="small">
          Logo and background URLs are set at tenant creation; the API does not
          expose branding updates on PATCH yet.
        </Text>
        <TextField
          label="Logo URL"
          readOnly
          value={tenant.branding?.logoUrl ?? ''}
        />
        <TextField
          label="Background image URL"
          readOnly
          value={tenant.branding?.backgroundImageUrl ?? ''}
        />
      </Stack>

      <Stack tokens={{ childrenGap: 8 }}>
        <Text variant="large">Health score weights</Text>
        {(localWeightError ?? sumMsg) && (
          <MessageBar messageBarType={MessageBarType.blocked}>
            {localWeightError ?? sumMsg}
          </MessageBar>
        )}
        {weightRows.map((row, idx) => (
          <Stack horizontal key={idx} tokens={{ childrenGap: 8 }} verticalAlign="end">
            <TextField
              label={idx === 0 ? 'Key' : undefined}
              value={row.key}
              onChange={(_, v) => {
                const next = [...weightRows];
                next[idx] = { ...next[idx], key: v ?? '' };
                setWeightRows(next);
              }}
            />
            <TextField
              label={idx === 0 ? 'Weight' : undefined}
              value={row.value}
              onChange={(_, v) => {
                const next = [...weightRows];
                next[idx] = { ...next[idx], value: v ?? '' };
                setWeightRows(next);
              }}
            />
            <DefaultButton
              text="Remove"
              disabled={weightRows.length <= 1}
              onClick={() =>
                setWeightRows((r) => r.filter((_, i) => i !== idx))
              }
            />
          </Stack>
        ))}
        <DefaultButton
          text="Add weight row"
          onClick={() =>
            setWeightRows((r) => [...r, { key: '', value: '0' }])
          }
        />
      </Stack>

      <Stack tokens={{ childrenGap: 8 }}>
        <Text variant="large">Health status thresholds</Text>
        <TextField
          label="Healthy min"
          type="number"
          value={
            thresholds.healthyMin === undefined || thresholds.healthyMin === null
              ? ''
              : String(thresholds.healthyMin)
          }
          onChange={(_, v) =>
            setThresholds((t) => ({
              ...t,
              healthyMin: v === '' ? undefined : Number.parseFloat(v ?? ''),
            }))
          }
        />
        <TextField
          label="Warning min"
          type="number"
          value={
            thresholds.warningMin === undefined || thresholds.warningMin === null
              ? ''
              : String(thresholds.warningMin)
          }
          onChange={(_, v) =>
            setThresholds((t) => ({
              ...t,
              warningMin: v === '' ? undefined : Number.parseFloat(v ?? ''),
            }))
          }
        />
        <TextField
          label="Critical below"
          type="number"
          value={
            thresholds.criticalBelow === undefined ||
            thresholds.criticalBelow === null
              ? ''
              : String(thresholds.criticalBelow)
          }
          onChange={(_, v) =>
            setThresholds((t) => ({
              ...t,
              criticalBelow: v === '' ? undefined : Number.parseFloat(v ?? ''),
            }))
          }
        />
      </Stack>

      <PrimaryButton
        text="Save configuration"
        onClick={() => void onSaveConfig()}
        disabled={updateConfig.isPending}
      />

      {updateConfig.isError ? (
        <MessageBar messageBarType={MessageBarType.error}>
          Failed to save. Check values and try again.
        </MessageBar>
      ) : null}

      <Dialog
        hidden={!confirmDeactivate}
        onDismiss={() => setConfirmDeactivate(false)}
        dialogContentProps={{
          type: DialogType.normal,
          title: 'Deactivate tenant?',
          subText:
            'Users may lose access to this tenant until it is activated again.',
        }}
      >
        <DialogFooter>
          <PrimaryButton
            text="Deactivate"
            onClick={() => void onDeactivate()}
            disabled={deactivate.isPending}
          />
          <DefaultButton
            text="Cancel"
            onClick={() => setConfirmDeactivate(false)}
            disabled={deactivate.isPending}
          />
        </DialogFooter>
      </Dialog>
    </Stack>
  );
};

export default TenantDetailPage;
