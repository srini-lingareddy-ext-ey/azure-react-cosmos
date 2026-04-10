import { useQuery } from '@tanstack/react-query';
import { fetchJobsByExecution, fetchJobDetail, fetchJobHistory } from '../jobMonitoringService';
export function useJobsByExecution(executionId: string) { return useQuery({ queryKey: ['jobs', executionId], queryFn: () => fetchJobsByExecution(executionId), enabled: !!executionId }); }
export function useJobDetail(executionId: string, jobName: string) { return useQuery({ queryKey: ['jobDetail', executionId, jobName], queryFn: () => fetchJobDetail(executionId, jobName), enabled: !!executionId && !!jobName }); }
export function useJobHistory(pipelineId: string, jobName: string, days = 30) { return useQuery({ queryKey: ['jobHistory', pipelineId, jobName, days], queryFn: () => fetchJobHistory(pipelineId, jobName, days), enabled: !!pipelineId && !!jobName }); }
