export interface SLAWindowConfig { windowType: string; windowValue: number; atRiskBufferMinutes: number; }
export interface BusinessPlanResponse { id: string; tenantId: string; name: string; description?: string; domain?: string; isActive: boolean; defaultSlaWindow?: SLAWindowConfig; schemaVersion: number; createdAt?: string; updatedAt?: string; createdBy?: string; updatedBy?: string; }
export interface BusinessPlanListResponse { items: BusinessPlanResponse[]; totalCount: number; }
export interface CreateBusinessPlanRequest { name: string; description?: string; domain?: string; defaultSlaWindow?: SLAWindowConfig; }
export interface UpdateBusinessPlanRequest { name?: string; description?: string; domain?: string; defaultSlaWindow?: SLAWindowConfig; }
