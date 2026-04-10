import { apiClient } from '../../services/apiClient';

export interface IncidentListEntry {
  id: string;
  displayId: string;
  severity: string;
  state: string;
  monitorName: string;
  businessPlan: string;
  serviceNowTicketNumber?: string;
  ticketCreationStatus: string;
  createdAt?: string;
}

export interface IncidentListResponse {
  items: IncidentListEntry[];
  pagination: { total: number; hasMore: boolean };
}

export interface ServiceNowPanel {
  ticketNumber?: string;
  ticketUrl?: string;
  ticketStatus?: string;
  ticketCreationStatus: string;
  lastSyncedAt?: string;
}

export interface StateHistoryEntry {
  fromState?: string;
  toState: string;
  actor: string;
  timestamp: string;
  note?: string;
}

export interface IncidentNoteEntry {
  noteId: string;
  content: string;
  authorId: string;
  authorName: string;
  createdAt: string;
  syncedToServiceNow: boolean;
}

export interface IncidentDetail {
  id: string;
  displayId: string;
  severity: string;
  state: string;
  monitorId: string;
  monitorName: string;
  businessPlan: string;
  affectedPipelineId?: string;
  triggeringEventId: string;
  recurrenceCount: number;
  resolutionNote?: string;
  serviceNow: ServiceNowPanel;
  stateHistory: StateHistoryEntry[];
  notes: IncidentNoteEntry[];
  createdAt?: string;
  etag?: string;
}

export interface IncidentFilters {
  severity?: string;
  state?: string;
  from?: string;
  to?: string;
  sort?: string;
  order?: string;
  limit?: number;
  offset?: number;
}

export const fetchIncidents = async (filters: IncidentFilters): Promise<IncidentListResponse> =>
  (await apiClient.get<IncidentListResponse>('/api/v1/incidents', { params: filters })).data;

export const fetchIncidentDetail = async (id: string): Promise<IncidentDetail> =>
  (await apiClient.get<IncidentDetail>(`/api/v1/incidents/${id}`)).data;

export const transitionState = async (id: string, toState: string, resolutionNote?: string, etag?: string) =>
  (await apiClient.patch(`/api/v1/incidents/${id}/state`, { toState, resolutionNote }, { headers: etag ? { 'If-Match': etag } : {} })).data;

export const addNote = async (id: string, content: string) =>
  (await apiClient.post(`/api/v1/incidents/${id}/notes`, { content })).data;

export const retryTicket = async (id: string) =>
  (await apiClient.post(`/api/v1/incidents/${id}/retry-ticket`)).data;
