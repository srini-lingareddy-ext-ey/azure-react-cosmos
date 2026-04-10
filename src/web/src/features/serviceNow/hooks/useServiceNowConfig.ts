import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchServiceNowConfig, upsertServiceNowConfig } from '../serviceNowService';

export function useServiceNowConfig() {
  return useQuery({ queryKey: ['snConfig'], queryFn: fetchServiceNowConfig });
}

export function useUpsertServiceNowConfig() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: upsertServiceNowConfig, onSuccess: () => qc.invalidateQueries({ queryKey: ['snConfig'] }) });
}
