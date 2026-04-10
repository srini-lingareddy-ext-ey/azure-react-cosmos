import { apiClient } from '../../services/apiClient';
import type { QueryTemplateListResponse, QueryTemplateResponse, CreateQueryTemplateRequest, UpdateQueryTemplateRequest } from './queryTemplateTypes';
const BASE = '/api/v1/admin/query-templates';
export const listQueryTemplates = async (): Promise<QueryTemplateListResponse> => (await apiClient.get<QueryTemplateListResponse>(BASE)).data;
export const getQueryTemplate = async (id: string): Promise<QueryTemplateResponse> => (await apiClient.get<QueryTemplateResponse>(`${BASE}/${id}`)).data;
export const createQueryTemplate = async (b: CreateQueryTemplateRequest): Promise<QueryTemplateResponse> => (await apiClient.post<QueryTemplateResponse>(BASE, b)).data;
export const updateQueryTemplate = async (id: string, b: UpdateQueryTemplateRequest): Promise<QueryTemplateResponse> => (await apiClient.patch<QueryTemplateResponse>(`${BASE}/${id}`, b)).data;
