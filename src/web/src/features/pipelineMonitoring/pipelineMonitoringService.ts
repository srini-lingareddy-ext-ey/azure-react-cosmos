import { apiClient } from '../../services/apiClient';
export interface PipelineStatusDto { pipelineId: string; pipelineName: string; businessPlan?: string; domain?: string; layer?: string; status: string; lastRunAt?: string; latestExecutionId?: string; hops: HopSummaryDto[]; }
export interface HopSummaryDto { layer: string; status: string; hasDetail: boolean; }
export interface HopDetailDto { layer: string; status: string; startTime?: string; endTime?: string; durationSeconds?: number; errorMessage?: string; sourceSystem?: string; }
export interface MemSQLInterfaceDto { interfaceName: string; status: string; pendingRecordCount: number; lastCompletedAt?: string; lastErrorMessage?: string; }
export interface PipelineStatusListResponse { data: PipelineStatusDto[]; pagination: { total: number; hasMore: boolean }; }
export const fetchPipelineStatus = async (params?: Record<string, string>): Promise<PipelineStatusListResponse> => (await apiClient.get<PipelineStatusListResponse>('/api/v1/pipelines/status', { params })).data;
export const fetchHopDetail = async (executionId: string, layer: string): Promise<HopDetailDto> => (await apiClient.get<HopDetailDto>(`/api/v1/pipelines/executions/${executionId}/hops/${layer}`)).data;
export const fetchMemSQLInterfaces = async (status?: string): Promise<MemSQLInterfaceDto[]> => (await apiClient.get<MemSQLInterfaceDto[]>('/api/v1/memsql/interfaces', { params: status ? { status } : {} })).data;
