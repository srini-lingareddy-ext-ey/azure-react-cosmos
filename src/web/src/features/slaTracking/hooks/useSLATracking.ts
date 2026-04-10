import { useQuery } from '@tanstack/react-query';
import { fetchSLAStatus, fetchSLACompliance, fetchSLAHistory } from '../slaTrackingService';
export const slaStatusKey = ['slaStatus'] as const;
export function useSLAStatus(status?: string) { return useQuery({ queryKey: [...slaStatusKey, status], queryFn: () => fetchSLAStatus(status), refetchInterval: 300000 }); }
export function useSLACompliance(timeRange?: string) { return useQuery({ queryKey: ['slaCompliance', timeRange], queryFn: () => fetchSLACompliance(timeRange), refetchInterval: 300000 }); }
export function useSLAHistory(pipelineId: string, limit = 30) { return useQuery({ queryKey: ['slaHistory', pipelineId, limit], queryFn: () => fetchSLAHistory(pipelineId, limit), enabled: !!pipelineId }); }
