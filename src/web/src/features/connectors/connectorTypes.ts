export type IntegrationMode = 'Polling' | 'Push';
export type ExecutionStatus = 'Success' | 'Failed' | 'Partial';
export interface FieldMapping { sourceField: string; targetField: string; transformType: string; valueMap?: Record<string, string>; }
export interface ConnectorResponse { id: string; tenantId: string; connectorName: string; connectorTypeId: string; isEnabled: boolean; integrationMode: IntegrationMode; pollingScheduleCron?: string; fieldMappings?: FieldMapping[]; createdAt?: string; updatedAt?: string; }
export interface ConnectorListResponse { items: ConnectorResponse[]; totalCount: number; }
export interface ConnectorTestResponse { success: boolean; errorMessage?: string; }
export interface ConnectorLogEntry { id: string; executedAt: string; status: ExecutionStatus; eventsProduced: number; durationMs: number; errorMessage?: string; }
export interface ConnectorLogResponse { entries: ConnectorLogEntry[]; successRateLast30Cycles: number; }
export interface ConnectorTypeCatalogEntry { connectorTypeId: string; displayName: string; integrationMode: IntegrationMode; certificationStatus: string; requiredCredentialFields: string[]; }
export interface CreateConnectorRequest { connectorName: string; connectorTypeId: string; integrationMode: IntegrationMode; pollingScheduleCron?: string; credentials: string; fieldMappings?: FieldMapping[]; }
