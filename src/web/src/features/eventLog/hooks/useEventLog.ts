import { useQuery } from '@tanstack/react-query';
import { fetchEvents, fetchEventDetail, fetchClassificationRule } from '../eventLogService';
import type { EventLogFilters } from '../eventLogService';

export function useEventLog(filters: EventLogFilters) {
  return useQuery({
    queryKey: ['eventLog', filters],
    queryFn: () => fetchEvents(filters),
    refetchInterval: 300_000,
  });
}

export function useEventDetail(eventId: string) {
  return useQuery({
    queryKey: ['eventDetail', eventId],
    queryFn: () => fetchEventDetail(eventId),
    enabled: !!eventId,
  });
}

export function useClassificationRule(ruleId: string | null | undefined) {
  return useQuery({
    queryKey: ['classificationRule', ruleId],
    queryFn: () => fetchClassificationRule(ruleId!),
    enabled: !!ruleId && ruleId !== 'default' && ruleId !== 'error',
  });
}
