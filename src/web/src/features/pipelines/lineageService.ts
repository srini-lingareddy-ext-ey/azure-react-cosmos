import { apiClient } from '../../services/apiClient';
export interface LineageEdge { relationshipId: string; relatedPipelineId: string; relatedPipelineName: string; }
export interface PipelineLineageResponse { pipelineId: string; upstream: LineageEdge[]; downstream: LineageEdge[]; }
export interface CreateLineageRequest { upstreamPipelineId: string; downstreamPipelineId: string; }
export const getLineage = async (pipelineId: string): Promise<PipelineLineageResponse> => (await apiClient.get<PipelineLineageResponse>(`/api/v1/admin/pipelines/${pipelineId}/lineage`)).data;
export const createLineage = async (body: CreateLineageRequest) => (await apiClient.post('/api/v1/admin/lineage', body)).data;
export const deleteLineage = async (id: string) => { await apiClient.delete(`/api/v1/admin/lineage/${id}`); };
