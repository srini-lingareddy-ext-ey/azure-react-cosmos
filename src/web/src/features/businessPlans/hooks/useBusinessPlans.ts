import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { listBusinessPlans, getBusinessPlan, createBusinessPlan, updateBusinessPlan, activateBusinessPlan, deactivateBusinessPlan } from '../businessPlanService';
import type { CreateBusinessPlanRequest, UpdateBusinessPlanRequest } from '../businessPlanTypes';

export const businessPlansKey = ['businessPlans'] as const;
export const businessPlanKey = (id: string) => ['businessPlan', id] as const;

export function useBusinessPlans(isActive?: boolean) { return useQuery({ queryKey: [...businessPlansKey, isActive], queryFn: () => listBusinessPlans(isActive) }); }
export function useBusinessPlan(id: string | undefined) { return useQuery({ queryKey: businessPlanKey(id ?? '_'), queryFn: () => getBusinessPlan(id!), enabled: Boolean(id) }); }
export function useCreateBusinessPlan() { const qc = useQueryClient(); return useMutation({ mutationFn: (body: CreateBusinessPlanRequest) => createBusinessPlan(body), onSuccess: () => { void qc.invalidateQueries({ queryKey: businessPlansKey }); } }); }
export function useUpdateBusinessPlan(id: string) { const qc = useQueryClient(); return useMutation({ mutationFn: (body: UpdateBusinessPlanRequest) => updateBusinessPlan(id, body), onSuccess: () => { void qc.invalidateQueries({ queryKey: businessPlansKey }); void qc.invalidateQueries({ queryKey: businessPlanKey(id) }); } }); }
export function useActivateBusinessPlan(id: string) { const qc = useQueryClient(); return useMutation({ mutationFn: () => activateBusinessPlan(id), onSuccess: () => { void qc.invalidateQueries({ queryKey: businessPlansKey }); void qc.invalidateQueries({ queryKey: businessPlanKey(id) }); } }); }
export function useDeactivateBusinessPlan(id: string) { const qc = useQueryClient(); return useMutation({ mutationFn: () => deactivateBusinessPlan(id), onSuccess: () => { void qc.invalidateQueries({ queryKey: businessPlansKey }); void qc.invalidateQueries({ queryKey: businessPlanKey(id) }); } }); }
