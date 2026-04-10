import { apiClient } from '../../services/apiClient';
import type { BusinessPlanListResponse, BusinessPlanResponse, CreateBusinessPlanRequest, UpdateBusinessPlanRequest } from './businessPlanTypes';

const BASE = '/api/v1/admin/business-plans';

export const listBusinessPlans = async (isActive?: boolean): Promise<BusinessPlanListResponse> => {
  const res = await apiClient.get<BusinessPlanListResponse>(BASE, { params: isActive !== undefined ? { isActive } : {} });
  return res.data;
};
export const getBusinessPlan = async (id: string): Promise<BusinessPlanResponse> => (await apiClient.get<BusinessPlanResponse>(`${BASE}/${id}`)).data;
export const createBusinessPlan = async (body: CreateBusinessPlanRequest): Promise<BusinessPlanResponse> => (await apiClient.post<BusinessPlanResponse>(BASE, body)).data;
export const updateBusinessPlan = async (id: string, body: UpdateBusinessPlanRequest): Promise<BusinessPlanResponse> => (await apiClient.patch<BusinessPlanResponse>(`${BASE}/${id}`, body)).data;
export const activateBusinessPlan = async (id: string): Promise<BusinessPlanResponse> => (await apiClient.post<BusinessPlanResponse>(`${BASE}/${id}/activate`)).data;
export const deactivateBusinessPlan = async (id: string): Promise<BusinessPlanResponse> => (await apiClient.post<BusinessPlanResponse>(`${BASE}/${id}/deactivate`)).data;
