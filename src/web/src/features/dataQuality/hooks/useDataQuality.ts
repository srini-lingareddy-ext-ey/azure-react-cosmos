import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { fetchDQStatus, fetchDQTrend, fetchDQChecks, upsertDQConfig } from '../dataQualityService';
import type { DataQualityThresholdRequest } from '../dataQualityService';
export const dqStatusKey = ['dqStatus'] as const;
export function useDQStatus(qualityStatus?: string) { return useQuery({ queryKey: [...dqStatusKey, qualityStatus], queryFn: () => fetchDQStatus(qualityStatus), refetchInterval: 300000 }); }
export function useDQTrend(pipelineId: string, days = 7) { return useQuery({ queryKey: ['dqTrend', pipelineId, days], queryFn: () => fetchDQTrend(pipelineId, days), enabled: !!pipelineId }); }
export function useDQChecks(pipelineId: string, scoreId: string) { return useQuery({ queryKey: ['dqChecks', pipelineId, scoreId], queryFn: () => fetchDQChecks(pipelineId, scoreId), enabled: !!pipelineId && !!scoreId }); }
export function useUpsertDQConfig(pipelineId: string) { const qc = useQueryClient(); return useMutation({ mutationFn: (config: DataQualityThresholdRequest) => upsertDQConfig(pipelineId, config), onSuccess: () => { void qc.invalidateQueries({ queryKey: dqStatusKey }); } }); }
