import { apiClient } from '../../services/apiClient';

export interface ServiceNowConfig {
  endpointUrl: string;
  authType: string;
  callerUserId?: string;
  ticketTemplate?: string;
  urgencyMapping: Record<string, number>;
  severityMapping: Record<string, string>;
  stateMapping: Record<string, string>;
}

export interface UpsertServiceNowConfig {
  endpointUrl?: string;
  authType?: string;
  credentialSecretName?: string;
  callerUserId?: string;
  ticketTemplate?: string;
  urgencyMapping?: Record<string, number>;
  severityMapping?: Record<string, string>;
  stateMapping?: Record<string, string>;
}

export const fetchServiceNowConfig = async (): Promise<ServiceNowConfig | null> => {
  try { return (await apiClient.get('/api/v1/admin/servicenow-config')).data; }
  catch { return null; }
};

export const upsertServiceNowConfig = async (data: UpsertServiceNowConfig): Promise<ServiceNowConfig> =>
  (await apiClient.post('/api/v1/admin/servicenow-config', data)).data;
