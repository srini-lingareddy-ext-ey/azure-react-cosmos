import { useQuery } from '@tanstack/react-query';
import {
  fetchSLAStatus,
  fetchSLACompliance,
  fetchSLATrend,
  fetchSLADrillDown,
  fetchSLAHistory,
} from '../slaService';

export const slaKeys = {
  status: (status?: string) => ['slaStatus', status] as const,
  compliance: (timeRange?: string) => ['slaCompliance', timeRange] as const,
  trend: (days: number) => ['slaTrend', days] as const,
  drillDown: (bp: string, limit: number) => ['slaDrillDown', bp, limit] as const,
  history: (pipelineId: string, limit: number) =>
    ['slaHistory', pipelineId, limit] as const,
};

export function useSLAStatus(status?: string) {
  return useQuery({
    queryKey: slaKeys.status(status),
    queryFn: () => fetchSLAStatus(status),
    refetchInterval: 300_000,
  });
}

export function useSLACompliance(timeRange?: string) {
  return useQuery({
    queryKey: slaKeys.compliance(timeRange),
    queryFn: () => fetchSLACompliance(timeRange),
    refetchInterval: 300_000,
  });
}

export function useSLATrend(days: number = 7) {
  return useQuery({
    queryKey: slaKeys.trend(days),
    queryFn: () => fetchSLATrend(days),
    refetchInterval: 300_000,
  });
}

export function useSLADrillDown(businessPlan: string, limit: number = 30) {
  return useQuery({
    queryKey: slaKeys.drillDown(businessPlan, limit),
    queryFn: () => fetchSLADrillDown(businessPlan, limit),
    enabled: !!businessPlan,
  });
}

export function useSLAHistory(pipelineId: string, limit: number = 30) {
  return useQuery({
    queryKey: slaKeys.history(pipelineId, limit),
    queryFn: () => fetchSLAHistory(pipelineId, limit),
    enabled: !!pipelineId,
  });
}
