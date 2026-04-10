import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { listMonitors, pauseMonitor, activateMonitor } from '../monitorService';
export const monitorsKey = ['monitors'] as const;
export function useMonitors(status?: string, bpId?: string) { return useQuery({ queryKey: [...monitorsKey, status, bpId], queryFn: () => listMonitors(status, bpId) }); }
export function usePauseMonitor(id: string) { const qc = useQueryClient(); return useMutation({ mutationFn: () => pauseMonitor(id), onSuccess: () => { void qc.invalidateQueries({ queryKey: monitorsKey }); } }); }
export function useActivateMonitor(id: string) { const qc = useQueryClient(); return useMutation({ mutationFn: () => activateMonitor(id), onSuccess: () => { void qc.invalidateQueries({ queryKey: monitorsKey }); } }); }
