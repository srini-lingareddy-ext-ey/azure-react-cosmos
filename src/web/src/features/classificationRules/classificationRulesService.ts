import { apiClient } from '../../services/apiClient';

export interface ClassificationRuleView {
  ruleId: string;
  priority: number;
  description: string;
  outcome: string;
  deployedAt?: string;
  conditions: { field: string; operator: string; value: string }[];
}

export interface ClassificationAuditEntry {
  id: string;
  eventId: string;
  matchedRuleId: string;
  outcome: string;
  classifiedAt?: string;
  eventType: string;
  sourceSystem: string;
  monitorName: string;
}

export const fetchClassificationRules = async (): Promise<ClassificationRuleView[]> =>
  (await apiClient.get<ClassificationRuleView[]>('/api/v1/classification-rules')).data;

export const fetchClassificationAuditLog = async (
  outcome?: string, from?: string, to?: string, limit = 50, offset = 0
): Promise<ClassificationAuditEntry[]> =>
  (await apiClient.get<ClassificationAuditEntry[]>('/api/v1/classification-rules/audit-log', {
    params: { outcome, from, to, limit, offset },
  })).data;
