import { apiClient } from '../../services/apiClient';
export interface ComponentHealthDto { componentId: string; componentName: string; componentType: string; status: string; lastMetricReceivedAt?: string; isStale: boolean; nodeCount: number; unhealthyNodeCount: number; evaluatedAt?: string; }
export interface ProductHealthDto { productId: string; productName: string; availability24h: number; status: string; lastHeartbeatAt?: string; isStale: boolean; }
export interface InfraStatusResponse { components: ComponentHealthDto[]; products: ProductHealthDto[]; }
export interface NodeStatusDto { nodeId: string; nodeName: string; status: string; lastMetricReceivedAt?: string; isStale: boolean; }
export interface SparklinePoint { timestamp: string; value: number; }
export interface NodeMetricDto { metricName: string; displayName?: string; unit?: string; currentValue?: number; warningThreshold?: number; criticalThreshold?: number; status: string; sparkline: SparklinePoint[]; }
export interface ProductAvailabilityResponse { availability24h: number; status: string; trend: { date: string; availabilityPercent: number }[]; }
export const fetchInfraStatus = async (status?: string): Promise<InfraStatusResponse> => (await apiClient.get<InfraStatusResponse>('/api/v1/infrastructure/status', { params: status ? { status } : {} })).data;
export const fetchComponentNodes = async (componentId: string): Promise<NodeStatusDto[]> => (await apiClient.get<NodeStatusDto[]>(`/api/v1/infrastructure/components/${componentId}/nodes`)).data;
export const fetchNodeMetrics = async (nodeId: string): Promise<NodeMetricDto[]> => (await apiClient.get<NodeMetricDto[]>(`/api/v1/infrastructure/nodes/${nodeId}/metrics`)).data;
export const fetchProductAvailability = async (productId: string, trendDays = 30): Promise<ProductAvailabilityResponse> => (await apiClient.get<ProductAvailabilityResponse>(`/api/v1/infrastructure/products/${productId}/availability`, { params: { trendDays } })).data;
