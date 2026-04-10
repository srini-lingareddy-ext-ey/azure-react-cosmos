import { apiClient } from '../../services/apiClient';

export interface MonitoredArtifactView {
  artifactId: string;
  artifactName: string;
  artifactType: string;
  currentStatus: string;
  lastScannedAt?: string;
  lastDeviationDetectedAt?: string;
}

export interface FingerprintAuditEntryView {
  id: string;
  artifactId: string;
  artifactName: string;
  artifactType: string;
  detectedAt?: string;
  changedBy: string;
  beforeHash: string;
  afterHash: string;
  changeClassification: string;
  approvedWindowName?: string;
  syncedToImmutableStorage: boolean;
}

export interface ApprovedWindowView {
  id: string;
  name: string;
  startTime: string;
  endTime: string;
  scopeType: string;
  scopeValue?: string;
}

export const fetchArtifacts = async (): Promise<MonitoredArtifactView[]> =>
  (await apiClient.get<MonitoredArtifactView[]>('/api/v1/compliance/fingerprints/artifacts')).data;

export const registerArtifact = async (data: { artifactName: string; artifactType: string; connectorId: string }): Promise<MonitoredArtifactView> =>
  (await apiClient.post<MonitoredArtifactView>('/api/v1/compliance/fingerprints/artifacts', data)).data;

export const triggerScan = async (artifactId: string): Promise<void> => {
  await apiClient.post(`/api/v1/compliance/fingerprints/artifacts/${artifactId}/scan`);
};

export const resetBaseline = async (artifactId: string, justification: string): Promise<void> => {
  await apiClient.post(`/api/v1/compliance/fingerprints/artifacts/${artifactId}/reset-baseline`, { justification });
};

export const fetchAuditTrail = async (changeClassification?: string, limit = 50, offset = 0): Promise<FingerprintAuditEntryView[]> =>
  (await apiClient.get<FingerprintAuditEntryView[]>('/api/v1/compliance/fingerprints/audit-trail', { params: { changeClassification, limit, offset } })).data;

export const fetchWindows = async (): Promise<ApprovedWindowView[]> =>
  (await apiClient.get<ApprovedWindowView[]>('/api/v1/compliance/fingerprints/windows')).data;

export const createWindow = async (data: { name: string; startTime: string; endTime: string; scopeType: string; scopeValue?: string }): Promise<ApprovedWindowView> =>
  (await apiClient.post<ApprovedWindowView>('/api/v1/compliance/fingerprints/windows', data)).data;

export const deleteWindow = async (windowId: string): Promise<void> => {
  await apiClient.delete(`/api/v1/compliance/fingerprints/windows/${windowId}`);
};
