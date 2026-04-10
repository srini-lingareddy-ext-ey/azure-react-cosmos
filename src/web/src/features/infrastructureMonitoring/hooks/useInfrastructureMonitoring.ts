import { useQuery } from '@tanstack/react-query';
import { fetchInfraStatus, fetchComponentNodes, fetchNodeMetrics, fetchProductAvailability } from '../infrastructureMonitoringService';
export const infraStatusKey = ['infraStatus'] as const;
export function useInfraStatus(status?: string) { return useQuery({ queryKey: [...infraStatusKey, status], queryFn: () => fetchInfraStatus(status), refetchInterval: 300000 }); }
export function useComponentNodes(componentId: string) { return useQuery({ queryKey: ['componentNodes', componentId], queryFn: () => fetchComponentNodes(componentId), enabled: !!componentId }); }
export function useNodeMetrics(nodeId: string) { return useQuery({ queryKey: ['nodeMetrics', nodeId], queryFn: () => fetchNodeMetrics(nodeId), enabled: !!nodeId }); }
export function useProductAvailability(productId: string, trendDays = 30) { return useQuery({ queryKey: ['productAvailability', productId, trendDays], queryFn: () => fetchProductAvailability(productId, trendDays), enabled: !!productId }); }
