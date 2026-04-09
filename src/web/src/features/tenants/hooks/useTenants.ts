import {
  useMutation,
  useQuery,
  useQueryClient,
  QueryClient,
} from '@tanstack/react-query';
import {
  activateTenant,
  createTenant,
  deactivateTenant,
  fetchAllTenants,
  getTenant,
  patchTenantConfig,
} from '../tenantService';
import type {
  CreateTenantRequest,
  TenantResponse,
  UpdateTenantConfigRequest,
} from '../tenantTypes';

export const tenantsQueryKey = ['tenants'] as const;
export const tenantQueryKey = (id: string) => ['tenant', id] as const;

export function useTenants() {
  return useQuery({
    queryKey: tenantsQueryKey,
    queryFn: fetchAllTenants,
  });
}

export function useTenant(id: string | undefined) {
  return useQuery({
    queryKey: tenantQueryKey(id ?? '_'),
    queryFn: () => getTenant(id!),
    enabled: Boolean(id),
  });
}

export function useCreateTenant() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateTenantRequest) => createTenant(body),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: tenantsQueryKey });
    },
  });
}

export function useUpdateTenantConfig(tenantId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateTenantConfigRequest) =>
      patchTenantConfig(tenantId, body),
    onSuccess: (data) => {
      void qc.invalidateQueries({ queryKey: tenantsQueryKey });
      void qc.invalidateQueries({ queryKey: tenantQueryKey(data.id) });
    },
  });
}

function applyStatusOptimistic(
  qc: QueryClient,
  id: string,
  status: TenantResponse['status']
) {
  qc.setQueryData<TenantResponse>(tenantQueryKey(id), (prev) =>
    prev ? { ...prev, status } : prev
  );
  qc.setQueryData<TenantResponse[]>(tenantsQueryKey, (prev) =>
    prev?.map((t) => (t.id === id ? { ...t, status } : t))
  );
}

export function useActivateTenant(tenantId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () => activateTenant(tenantId),
    onMutate: async () => {
      await qc.cancelQueries({ queryKey: tenantQueryKey(tenantId) });
      const prevTenant = qc.getQueryData<TenantResponse>(
        tenantQueryKey(tenantId)
      );
      const prevList = qc.getQueryData<TenantResponse[]>(tenantsQueryKey);
      applyStatusOptimistic(qc, tenantId, 'Active');
      return { prevTenant, prevList };
    },
    onError: (_e, _v, ctx) => {
      if (ctx?.prevTenant) {
        qc.setQueryData(tenantQueryKey(tenantId), ctx.prevTenant);
      } else {
        void qc.invalidateQueries({ queryKey: tenantQueryKey(tenantId) });
      }
      if (ctx?.prevList) {
        qc.setQueryData(tenantsQueryKey, ctx.prevList);
      } else {
        void qc.invalidateQueries({ queryKey: tenantsQueryKey });
      }
    },
    onSettled: () => {
      void qc.invalidateQueries({ queryKey: tenantQueryKey(tenantId) });
      void qc.invalidateQueries({ queryKey: tenantsQueryKey });
    },
  });
}

export function useDeactivateTenant(tenantId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () => deactivateTenant(tenantId),
    onMutate: async () => {
      await qc.cancelQueries({ queryKey: tenantQueryKey(tenantId) });
      const prevTenant = qc.getQueryData<TenantResponse>(
        tenantQueryKey(tenantId)
      );
      const prevList = qc.getQueryData<TenantResponse[]>(tenantsQueryKey);
      applyStatusOptimistic(qc, tenantId, 'Inactive');
      return { prevTenant, prevList };
    },
    onError: (_e, _v, ctx) => {
      if (ctx?.prevTenant) {
        qc.setQueryData(tenantQueryKey(tenantId), ctx.prevTenant);
      } else {
        void qc.invalidateQueries({ queryKey: tenantQueryKey(tenantId) });
      }
      if (ctx?.prevList) {
        qc.setQueryData(tenantsQueryKey, ctx.prevList);
      } else {
        void qc.invalidateQueries({ queryKey: tenantsQueryKey });
      }
    },
    onSettled: () => {
      void qc.invalidateQueries({ queryKey: tenantQueryKey(tenantId) });
      void qc.invalidateQueries({ queryKey: tenantsQueryKey });
    },
  });
}
