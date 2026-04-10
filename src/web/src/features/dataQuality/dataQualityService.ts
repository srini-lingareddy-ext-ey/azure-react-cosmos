import { apiClient } from '../../services/apiClient';
export interface DataQualityStatusDto { pipelineId: string; pipelineName: string; businessPlan?: string; domain?: string; qualityScore?: number; scoreTimestamp?: string; qualityStatus: string; latencyStatus: string; lastSuccessfulRunAt?: string; evaluatedAt?: string; }
export interface DataQualityTrendPoint { date: string; score?: number; scoreId?: string; }
export interface DataQualityCheck { checkName: string; passed: boolean; recordsEvaluated: number; recordsFailed: number; failureRate: number; message?: string; }
export interface DataQualityThresholdRequest { warningThreshold: number; criticalThreshold: number; freshnessThresholdSeconds?: number; freshnessBufferPercent?: number; }
export const fetchDQStatus = async (qualityStatus?: string): Promise<DataQualityStatusDto[]> => (await apiClient.get<DataQualityStatusDto[]>('/api/v1/data-quality/status', { params: qualityStatus ? { qualityStatus } : {} })).data;
export const fetchDQTrend = async (pipelineId: string, days = 7): Promise<DataQualityTrendPoint[]> => (await apiClient.get<DataQualityTrendPoint[]>(`/api/v1/data-quality/${pipelineId}/trend`, { params: { days } })).data;
export const fetchDQChecks = async (pipelineId: string, scoreId: string): Promise<DataQualityCheck[]> => (await apiClient.get<DataQualityCheck[]>(`/api/v1/data-quality/${pipelineId}/scores/${scoreId}/checks`)).data;
export const upsertDQConfig = async (pipelineId: string, config: DataQualityThresholdRequest): Promise<void> => { await apiClient.post(`/api/v1/data-quality/config/${pipelineId}`, config); };
