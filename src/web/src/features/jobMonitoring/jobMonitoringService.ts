import { apiClient } from '../../services/apiClient';
export interface JobRunDto { jobName: string; status: string; startTime?: string; endTime?: string; durationSeconds?: number; retryCount: number; isLongRunning: boolean; isSkipped: boolean; hasGranularData: boolean; errorMessage?: string; stackTrace?: string; sourceSystemUrl?: string; }
export interface JobHistorySummary { totalRuns: number; successRate: number; averageDurationSeconds?: number; }
export interface JobHistoryResponse { summary: JobHistorySummary; history: JobRunDto[]; }
export const fetchJobsByExecution = async (executionId: string): Promise<JobRunDto[]> => (await apiClient.get<JobRunDto[]>(`/api/v1/pipelines/executions/${executionId}/jobs`)).data;
export const fetchJobDetail = async (executionId: string, jobName: string): Promise<JobRunDto> => (await apiClient.get<JobRunDto>(`/api/v1/pipelines/executions/${executionId}/jobs/${jobName}`)).data;
export const fetchJobHistory = async (pipelineId: string, jobName: string, days = 30): Promise<JobHistoryResponse> => (await apiClient.get<JobHistoryResponse>(`/api/v1/pipelines/${pipelineId}/jobs/${jobName}/history`, { params: { days } })).data;
