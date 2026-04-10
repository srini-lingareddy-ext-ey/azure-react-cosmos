import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchIncidents, fetchIncidentDetail, transitionState, addNote, retryTicket } from '../incidentService';
import type { IncidentFilters } from '../incidentService';

export function useIncidentList(filters: IncidentFilters) {
  return useQuery({
    queryKey: ['incidents', filters],
    queryFn: () => fetchIncidents(filters),
    refetchInterval: 60_000,
  });
}

export function useIncidentDetail(id: string) {
  return useQuery({
    queryKey: ['incidentDetail', id],
    queryFn: () => fetchIncidentDetail(id),
    enabled: !!id,
  });
}

export function useTransitionState() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: { id: string; toState: string; resolutionNote?: string; etag?: string }) =>
      transitionState(args.id, args.toState, args.resolutionNote, args.etag),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['incidents'] }); qc.invalidateQueries({ queryKey: ['incidentDetail'] }); },
  });
}

export function useAddNote() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: { id: string; content: string }) => addNote(args.id, args.content),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['incidentDetail'] }); },
  });
}

export function useRetryTicket() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => retryTicket(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['incidentDetail'] }); },
  });
}
