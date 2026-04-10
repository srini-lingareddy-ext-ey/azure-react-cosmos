import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { getLineage, createLineage, deleteLineage, type CreateLineageRequest } from '../lineageService';
export const lineageKey = (id: string) => ['lineage', id] as const;
export function useLineage(pipelineId: string | undefined) { return useQuery({ queryKey: lineageKey(pipelineId ?? '_'), queryFn: () => getLineage(pipelineId!), enabled: Boolean(pipelineId) }); }
export function useCreateLineage(pipelineId: string) { const qc = useQueryClient(); return useMutation({ mutationFn: (b: CreateLineageRequest) => createLineage(b), onSuccess: () => { void qc.invalidateQueries({ queryKey: lineageKey(pipelineId) }); } }); }
export function useDeleteLineage(pipelineId: string) { const qc = useQueryClient(); return useMutation({ mutationFn: (id: string) => deleteLineage(id), onSuccess: () => { void qc.invalidateQueries({ queryKey: lineageKey(pipelineId) }); } }); }
