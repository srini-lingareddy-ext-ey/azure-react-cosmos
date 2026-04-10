import { apiClient } from '../../services/apiClient';
import type { PipelineRegistrationListResponse, PipelineRegistrationResponse, PipelineDeactivateResponse, CreatePipelineRegistrationRequest, UpdatePipelineRegistrationRequest } from './pipelineTypes';
const BASE = '/api/v1/admin/pipelines';
export const listPipelines = async (businessPlanId?: string, medallionLayer?: string): Promise<PipelineRegistrationListResponse> => { const res = await apiClient.get<PipelineRegistrationListResponse>(BASE, { params: { businessPlanId, medallionLayer } }); return res.data; };
export const getPipeline = async (id: string): Promise<PipelineRegistrationResponse> => (await apiClient.get<PipelineRegistrationResponse>(`${BASE}/${id}`)).data;
export const createPipeline = async (body: CreatePipelineRegistrationRequest): Promise<PipelineRegistrationResponse> => (await apiClient.post<PipelineRegistrationResponse>(BASE, body)).data;
export const updatePipeline = async (id: string, body: UpdatePipelineRegistrationRequest): Promise<PipelineRegistrationResponse> => (await apiClient.patch<PipelineRegistrationResponse>(`${BASE}/${id}`, body)).data;
export const activatePipeline = async (id: string): Promise<PipelineRegistrationResponse> => (await apiClient.post<PipelineRegistrationResponse>(`${BASE}/${id}/activate`)).data;
export const deactivatePipeline = async (id: string): Promise<PipelineDeactivateResponse> => (await apiClient.post<PipelineDeactivateResponse>(`${BASE}/${id}/deactivate`)).data;
