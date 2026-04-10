import { apiClient } from '../../services/apiClient';
export interface SLAStatusDto { pipelineId: string; pipelineName: string; businessPlan?: string; status: string; timeRemainingSeconds?: number; slaWindow?: string; lastRunAt?: string; evaluatedAt?: string; }
export interface SLAComplianceSummary { businessPlan: string; percentageMet: number; breachCount: number; }
export interface SLATrendPoint { date: string; met: number; breached: number; }
export interface SLAComplianceResponse { summary: SLAComplianceSummary[]; trend: SLATrendPoint[]; dataAvailabilityNote?: string; }
export interface SLABreachHistory { id: string; breachDetectedAt: string; slaWindowClosedAt: string; completedAt?: string; minutesOverdue?: number; }
export const fetchSLAStatus = async (status?: string): Promise<SLAStatusDto[]> => (await apiClient.get<SLAStatusDto[]>('/api/v1/sla/status', { params: status ? { status } : {} })).data;
export const fetchSLACompliance = async (timeRange?: string): Promise<SLAComplianceResponse> => (await apiClient.get<SLAComplianceResponse>('/api/v1/sla/compliance', { params: timeRange ? { timeRange } : {} })).data;
export const fetchSLAHistory = async (pipelineId: string, limit = 30): Promise<SLABreachHistory[]> => (await apiClient.get<SLABreachHistory[]>(`/api/v1/sla/history/${pipelineId}`, { params: { limit } })).data;
