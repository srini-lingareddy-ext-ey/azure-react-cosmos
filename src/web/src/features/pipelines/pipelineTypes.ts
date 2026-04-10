export type MedallionLayer = 'Raw' | 'Mirror' | 'Model' | 'Consumption';
export interface PipelineRegistrationResponse { id: string; tenantId: string; pipelineName: string; sourceSystem?: string; targetSystem?: string; medallionLayer: MedallionLayer; businessPlanId?: string; businessPlanName?: string; domain?: string; description?: string; isActive: boolean; schemaVersion: number; createdAt?: string; updatedAt?: string; }
export interface PipelineRegistrationListResponse { items: PipelineRegistrationResponse[]; totalCount: number; }
export interface PipelineDeactivateResponse { pipeline: PipelineRegistrationResponse; monitorsSuspended: number; }
export interface CreatePipelineRegistrationRequest { pipelineName: string; sourceSystem?: string; targetSystem?: string; medallionLayer: MedallionLayer; businessPlanId?: string; domain?: string; description?: string; }
export interface UpdatePipelineRegistrationRequest { pipelineName?: string; sourceSystem?: string; targetSystem?: string; medallionLayer?: MedallionLayer; businessPlanId?: string; domain?: string; description?: string; }
