import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchArtifacts, fetchAuditTrail, fetchWindows, triggerScan, resetBaseline, registerArtifact, createWindow, deleteWindow } from '../fingerprintingService';

export function useArtifacts() {
  return useQuery({ queryKey: ['fpArtifacts'], queryFn: fetchArtifacts });
}

export function useFingerprintAuditTrail(changeClassification?: string) {
  return useQuery({ queryKey: ['fpAuditTrail', changeClassification], queryFn: () => fetchAuditTrail(changeClassification) });
}

export function useApprovedWindows() {
  return useQuery({ queryKey: ['fpWindows'], queryFn: fetchWindows });
}

export function useTriggerScan() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: (artifactId: string) => triggerScan(artifactId), onSuccess: () => qc.invalidateQueries({ queryKey: ['fpArtifacts'] }) });
}

export function useResetBaseline() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: (args: { artifactId: string; justification: string }) => resetBaseline(args.artifactId, args.justification), onSuccess: () => { qc.invalidateQueries({ queryKey: ['fpArtifacts'] }); qc.invalidateQueries({ queryKey: ['fpAuditTrail'] }); } });
}

export function useRegisterArtifact() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: registerArtifact, onSuccess: () => qc.invalidateQueries({ queryKey: ['fpArtifacts'] }) });
}

export function useCreateWindow() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: createWindow, onSuccess: () => qc.invalidateQueries({ queryKey: ['fpWindows'] }) });
}

export function useDeleteWindow() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: deleteWindow, onSuccess: () => qc.invalidateQueries({ queryKey: ['fpWindows'] }) });
}
