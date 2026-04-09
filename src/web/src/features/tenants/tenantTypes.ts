/** Mirrors WO-9 tenant API JSON (camelCase). */

export type TenantStatus = 'Active' | 'Inactive';

export interface TenantBranding {
  logoUrl?: string | null;
  backgroundImageUrl?: string | null;
}

export interface HealthStatusThresholds {
  healthyMin?: number | null;
  warningMin?: number | null;
  criticalBelow?: number | null;
}

export interface TenantConfig {
  healthScoreWeights: Record<string, number>;
  healthStatusThresholds?: HealthStatusThresholds | null;
}

export interface TenantResponse {
  id: string;
  name: string;
  displayName: string;
  status: TenantStatus;
  branding?: TenantBranding | null;
  config?: TenantConfig | null;
  schemaVersion: number;
  createdAt?: string | null;
  updatedAt?: string | null;
}

export interface TenantListResponse {
  items: TenantResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CreateTenantRequest {
  name: string;
  displayName: string;
  logoUrl?: string | null;
  backgroundImageUrl?: string | null;
}

export interface UpdateTenantConfigRequest {
  healthScoreWeights?: Record<string, number> | null;
  healthStatusThresholds?: HealthStatusThresholds | null;
}
