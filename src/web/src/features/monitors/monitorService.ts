import { apiClient } from '../../services/apiClient';
import type { MonitorListResponse, MonitorResponse, CreateMonitorRequest } from './monitorTypes';
const BASE = '/api/v1/admin/monitors';
export const listMonitors = async (status?: string, businessPlanId?: string): Promise<MonitorListResponse> => (await apiClient.get<MonitorListResponse>(BASE, { params: { status, businessPlanId } })).data;
export const getMonitor = async (id: string): Promise<MonitorResponse> => (await apiClient.get<MonitorResponse>(`${BASE}/${id}`)).data;
export const createMonitor = async (b: CreateMonitorRequest): Promise<MonitorResponse> => (await apiClient.post<MonitorResponse>(BASE, b)).data;
export const pauseMonitor = async (id: string): Promise<MonitorResponse> => (await apiClient.post<MonitorResponse>(`${BASE}/${id}/pause`)).data;
export const activateMonitor = async (id: string): Promise<MonitorResponse> => (await apiClient.post<MonitorResponse>(`${BASE}/${id}/activate`)).data;
