import { useQuery } from '@tanstack/react-query';
import { fetchClassificationRules, fetchClassificationAuditLog } from '../classificationRulesService';

export function useClassificationRules() {
  return useQuery({ queryKey: ['classificationRules'], queryFn: fetchClassificationRules });
}

export function useClassificationAuditLog(outcome?: string, from?: string, to?: string, limit = 50, offset = 0) {
  return useQuery({
    queryKey: ['classificationAuditLog', outcome, from, to, limit, offset],
    queryFn: () => fetchClassificationAuditLog(outcome, from, to, limit, offset),
  });
}
