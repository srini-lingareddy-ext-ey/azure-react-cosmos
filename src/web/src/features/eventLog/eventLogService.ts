import { apiClient } from '../../services/apiClient';

export interface EventLogEntry {
  eventId: string;
  eventType: string;
  severity: string;
  classification: string;
  sourceSystem: string;
  monitorName: string;
  businessPlan?: string;
  timestamp?: string;
}

export interface EventDetail {
  eventId: string;
  eventType: string;
  severity: string;
  classification: string;
  classificationRuleId?: string;
  sourceSystem: string;
  connectorId: string;
  monitorId: string;
  monitorName: string;
  businessPlan?: string;
  pipelineId?: string;
  incidentId?: string;
  notificationStatus?: string;
  sourceTimestamp?: string;
  classifiedAt?: string;
  rawPayload: Record<string, unknown>;
}

export interface EventLogResponse {
  items: EventLogEntry[];
  pagination: { total: number; hasMore: boolean };
}

export interface ClassificationRuleDescription {
  ruleId: string;
  description: string;
}

export interface EventLogFilters {
  classification?: string;
  severity?: string;
  sourceSystem?: string;
  businessPlan?: string;
  from?: string;
  to?: string;
  limit?: number;
  offset?: number;
}

export const fetchEvents = async (filters: EventLogFilters): Promise<EventLogResponse> =>
  (await apiClient.get<EventLogResponse>('/api/v1/events', { params: filters })).data;

export const fetchEventDetail = async (eventId: string): Promise<EventDetail> =>
  (await apiClient.get<EventDetail>(`/api/v1/events/${eventId}`)).data;

export const fetchClassificationRule = async (ruleId: string): Promise<ClassificationRuleDescription> =>
  (await apiClient.get<ClassificationRuleDescription>(`/api/v1/classification-rules/${ruleId}`)).data;
