import { useQuery } from '@tanstack/react-query';
import { fetchPipelineStatus, fetchHopDetail, fetchMemSQLInterfaces } from '../pipelineMonitoringService';
export const pipelineStatusKey = ['pipelineStatus'] as const;
export function usePipelineStatus(status?: string, businessPlan?: string) {
  const params: Record<string, string> = {};
  if (status) params.status = status;
  if (businessPlan) params.businessPlan = businessPlan;
  return useQuery({ queryKey: [...pipelineStatusKey, status, businessPlan], queryFn: () => fetchPipelineStatus(params), refetchInterval: 300000 });
}
export function useHopDetail(executionId: string, layer: string) { return useQuery({ queryKey: ['hopDetail', executionId, layer], queryFn: () => fetchHopDetail(executionId, layer), enabled: !!executionId && !!layer }); }
export function useMemSQLInterfaces(status?: string) { return useQuery({ queryKey: ['memsqlInterfaces', status], queryFn: () => fetchMemSQLInterfaces(status), refetchInterval: 300000 }); }
