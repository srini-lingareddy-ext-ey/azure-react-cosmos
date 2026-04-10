import { apiClient } from '../../services/apiClient';
import type { ConnectionListResponse, ConnectionResponse, CreateConnectionRequest, UpdateConnectionRequest, ConnectionTestResponse } from './connectionTypes';
const BASE = '/api/v1/admin/connections';
export const listConnections = async (): Promise<ConnectionListResponse> => (await apiClient.get<ConnectionListResponse>(BASE)).data;
export const getConnection = async (id: string): Promise<ConnectionResponse> => (await apiClient.get<ConnectionResponse>(`${BASE}/${id}`)).data;
export const createConnection = async (b: CreateConnectionRequest): Promise<ConnectionResponse> => (await apiClient.post<ConnectionResponse>(BASE, b)).data;
export const updateConnection = async (id: string, b: UpdateConnectionRequest): Promise<ConnectionResponse> => (await apiClient.patch<ConnectionResponse>(`${BASE}/${id}`, b)).data;
export const deleteConnection = async (id: string): Promise<void> => { await apiClient.delete(`${BASE}/${id}`); };
export const testConnection = async (id: string): Promise<ConnectionTestResponse> => (await apiClient.post<ConnectionTestResponse>(`${BASE}/${id}/test`)).data;
