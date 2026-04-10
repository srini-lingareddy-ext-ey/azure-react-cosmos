import { apiClient } from '../../services/apiClient';

export interface ChannelEntry { id: string; name: string; type: string; isEnabled: boolean; }
export interface RoutingRuleEntry { id: string; name: string; isEnabled: boolean; scopeType: string; scopeValue?: string; classifications: string[]; severities: string[]; channelIds: string[]; }
export interface MaintenanceWindowEntry { id: string; name: string; startTime: string; endTime: string; scopeType: string; scopeValue?: string; }
export interface DeliveryLogEntry { id: string; eventId: string; channelName: string; channelType: string; recipient: string; deliveryStatus: string; attemptCount: number; sentAt?: string; errorMessage?: string; }

export const fetchChannels = async (): Promise<ChannelEntry[]> => (await apiClient.get('/api/v1/notifications/channels')).data;
export const createChannel = async (data: { name: string; type: string; isEnabled: boolean }) => (await apiClient.post('/api/v1/notifications/channels', data)).data;
export const deleteChannel = async (id: string) => apiClient.delete(`/api/v1/notifications/channels/${id}`);

export const fetchRoutingRules = async (): Promise<RoutingRuleEntry[]> => (await apiClient.get('/api/v1/notifications/routing-rules')).data;
export const createRoutingRule = async (data: { name: string; isEnabled: boolean; scopeType: string; scopeValue?: string; classifications?: string[]; severities?: string[]; channelIds?: string[] }) => (await apiClient.post('/api/v1/notifications/routing-rules', data)).data;
export const deleteRoutingRule = async (id: string) => apiClient.delete(`/api/v1/notifications/routing-rules/${id}`);

export const fetchMaintenanceWindows = async (): Promise<MaintenanceWindowEntry[]> => (await apiClient.get('/api/v1/notifications/maintenance-windows')).data;
export const createMaintenanceWindow = async (data: { name: string; startTime: string; endTime: string; scopeType: string; scopeValue?: string }) => (await apiClient.post('/api/v1/notifications/maintenance-windows', data)).data;
export const deleteMaintenanceWindow = async (id: string) => apiClient.delete(`/api/v1/notifications/maintenance-windows/${id}`);

export const fetchDeliveryLogs = async (params: { status?: string; from?: string; to?: string; limit?: number; offset?: number }): Promise<{ items: DeliveryLogEntry[] }> => (await apiClient.get('/api/v1/notifications/delivery-log', { params })).data;
