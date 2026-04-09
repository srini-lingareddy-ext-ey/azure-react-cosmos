import { apiClient } from '../../services/apiClient';
import type {
  CreateTenantRequest,
  TenantListResponse,
  TenantResponse,
  UpdateTenantConfigRequest,
} from './tenantTypes';

export async function listTenantsPage(
  page: number,
  pageSize: number
): Promise<TenantListResponse> {
  const res = await apiClient.get<TenantListResponse>('/api/v1/tenants', {
    params: { page, pageSize },
  });
  return res.data;
}

/**
 * Loads all tenant rows by paging (max page size 100 per API).
 * For very large directories, prefer a future API with filters or larger bulk endpoints.
 */
export async function fetchAllTenants(): Promise<TenantResponse[]> {
  const pageSize = 100;
  const first = await listTenantsPage(1, pageSize);
  const items = [...first.items];
  let page = 2;
  while (items.length < first.totalCount && first.totalCount > 0) {
    const next = await listTenantsPage(page, pageSize);
    if (next.items.length === 0) break;
    items.push(...next.items);
    page += 1;
    if (items.length >= first.totalCount) break;
  }
  return items;
}

export async function getTenant(id: string): Promise<TenantResponse> {
  const res = await apiClient.get<TenantResponse>(`/api/v1/tenants/${id}`);
  return res.data;
}

export async function createTenant(
  body: CreateTenantRequest
): Promise<TenantResponse> {
  const res = await apiClient.post<TenantResponse>('/api/v1/tenants', body);
  return res.data;
}

export async function patchTenantConfig(
  id: string,
  body: UpdateTenantConfigRequest
): Promise<TenantResponse> {
  const res = await apiClient.patch<TenantResponse>(
    `/api/v1/tenants/${id}/config`,
    body
  );
  return res.data;
}

export async function activateTenant(id: string): Promise<TenantResponse> {
  const res = await apiClient.post<TenantResponse>(
    `/api/v1/tenants/${id}/activate`
  );
  return res.data;
}

export async function deactivateTenant(id: string): Promise<TenantResponse> {
  const res = await apiClient.post<TenantResponse>(
    `/api/v1/tenants/${id}/deactivate`
  );
  return res.data;
}
