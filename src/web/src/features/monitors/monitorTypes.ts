export type MonitorEntityType = 'Pipeline' | 'InfrastructureComponent';
export type MonitorState = 'Active' | 'Paused' | 'Error';
export interface AlertThreshold { metricName: string; warningValue: number; criticalValue: number; operator: string; unit: string; }
export interface MonitorResponse { id: string; tenantId: string; monitorName: string; entityType: MonitorEntityType; entityId: string; entityName: string; businessPlanId?: string; businessPlanName?: string; connectionId: string; connectionName: string; queryTemplateId?: string; pollingFrequencyMinutes: number; alertThresholds?: AlertThreshold[]; status: MonitorState; createdAt?: string; updatedAt?: string; }
export interface MonitorListResponse { items: MonitorResponse[]; totalCount: number; }
export interface CreateMonitorRequest { monitorName: string; entityType: MonitorEntityType; entityId: string; entityName: string; businessPlanId?: string; connectionId: string; queryTemplateId?: string; pollingFrequencyMinutes: number; alertThresholds?: AlertThreshold[]; }
