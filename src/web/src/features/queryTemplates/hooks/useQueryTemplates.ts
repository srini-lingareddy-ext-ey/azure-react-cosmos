import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { listQueryTemplates, createQueryTemplate, updateQueryTemplate } from '../queryTemplateService';
import type { CreateQueryTemplateRequest, UpdateQueryTemplateRequest } from '../queryTemplateTypes';
export const qtKey = ['queryTemplates'] as const;
export function useQueryTemplates() { return useQuery({ queryKey: qtKey, queryFn: listQueryTemplates }); }
export function useCreateQueryTemplate() { const qc = useQueryClient(); return useMutation({ mutationFn: (b: CreateQueryTemplateRequest) => createQueryTemplate(b), onSuccess: () => { void qc.invalidateQueries({ queryKey: qtKey }); } }); }
export function useUpdateQueryTemplate(id: string) { const qc = useQueryClient(); return useMutation({ mutationFn: (b: UpdateQueryTemplateRequest) => updateQueryTemplate(id, b), onSuccess: () => { void qc.invalidateQueries({ queryKey: qtKey }); } }); }
