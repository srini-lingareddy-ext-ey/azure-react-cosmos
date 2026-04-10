import { apiClient } from '../../services/apiClient';

export interface SLAStatusDto {
  pipelineId: string;
  pipelineName: string;
  businessPlan?: string;
  status: string;
  timeRemainingSeconds?: number;
  completedAt?: string;
  slaWindow?: string;
  lastRunAt?: string;
  evaluatedAt?: string;
}

export interface SLAComplianceSummary {
  businessPlan: string;
  percentageMet: number;
  breachCount: number;
  atRiskCount: number;
}

export interface SLATrendPoint {
  date: string;
  complianceRate: number;
  businessPlan: string;
}

export interface SLAComplianceResponse {
  summary: SLAComplianceSummary[];
  trend: SLATrendPoint[];
  dataAvailabilityNote?: string;
}

export interface SLADrillDownExecution {
  executionId: string;
  status: string;
  startedAt: string;
}

export interface SLADrillDownPipeline {
  pipelineId: string;
  pipelineName: string;
  executions: SLADrillDownExecution[];
}

export interface SLABreachHistory {
  id: string;
  breachDetectedAt: string;
  slaWindowClosedAt: string;
  completedAt?: string;
  minutesOverdue?: number;
}

export const fetchSLAStatus = async (
  status?: string
): Promise<SLAStatusDto[]> =>
  (
    await apiClient.get<SLAStatusDto[]>('/api/v1/sla/status', {
      params: status ? { status } : {},
    })
  ).data;

export const fetchSLACompliance = async (
  timeRange?: string
): Promise<SLAComplianceResponse> =>
  (
    await apiClient.get<SLAComplianceResponse>('/api/v1/sla/compliance', {
      params: timeRange ? { timeRange } : {},
    })
  ).data;

export const fetchSLATrend = async (
  days: number = 7
): Promise<{ trend: SLATrendPoint[]; dataAvailabilityNote?: string }> =>
  (
    await apiClient.get<{ trend: SLATrendPoint[]; dataAvailabilityNote?: string }>(
      '/api/v1/sla/trend',
      { params: { days } }
    )
  ).data;

export const fetchSLADrillDown = async (
  businessPlan: string,
  limit: number = 30
): Promise<SLADrillDownPipeline[]> =>
  (
    await apiClient.get<SLADrillDownPipeline[]>(
      `/api/v1/sla/drilldown/${encodeURIComponent(businessPlan)}`,
      { params: { limit } }
    )
  ).data;

export const fetchSLAHistory = async (
  pipelineId: string,
  limit = 30
): Promise<SLABreachHistory[]> =>
  (
    await apiClient.get<SLABreachHistory[]>(
      `/api/v1/sla/history/${pipelineId}`,
      { params: { limit } }
    )
  ).data;
