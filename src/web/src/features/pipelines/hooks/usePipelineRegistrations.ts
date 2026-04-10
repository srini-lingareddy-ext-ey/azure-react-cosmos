import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { listPipelines, createPipeline, activatePipeline, deactivatePipeline } from '../pipelineService';
import type { CreatePipelineRegistrationRequest } from '../pipelineTypes';
export const pipelinesKey = ['pipelines'] as const;
export function usePipelines(bpId?: string, layer?: string) { return useQuery({ queryKey: [...pipelinesKey, bpId, layer], queryFn: () => listPipelines(bpId, layer) }); }
export function useCreatePipeline() { const qc = useQueryClient(); return useMutation({ mutationFn: (b: CreatePipelineRegistrationRequest) => createPipeline(b), onSuccess: () => { void qc.invalidateQueries({ queryKey: pipelinesKey }); } }); }
export function useActivatePipeline(id: string) { const qc = useQueryClient(); return useMutation({ mutationFn: () => activatePipeline(id), onSuccess: () => { void qc.invalidateQueries({ queryKey: pipelinesKey }); } }); }
export function useDeactivatePipeline(id: string) { const qc = useQueryClient(); return useMutation({ mutationFn: () => deactivatePipeline(id), onSuccess: () => { void qc.invalidateQueries({ queryKey: pipelinesKey }); } }); }
