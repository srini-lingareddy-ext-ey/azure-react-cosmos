export interface ConnectionResponse { id: string; tenantId: string; connectionName: string; connectorTypeId: string; isEnabled: boolean; lastTestedAt?: string; lastTestResult?: string; schemaVersion: number; createdAt?: string; updatedAt?: string; }
export interface ConnectionListResponse { items: ConnectionResponse[]; totalCount: number; }
export interface CreateConnectionRequest { connectionName: string; connectorTypeId: string; credentials: string; }
export interface UpdateConnectionRequest { connectionName?: string; connectorTypeId?: string; credentials?: string; isEnabled?: boolean; }
export interface ConnectionTestResponse { success: boolean; message?: string; }
