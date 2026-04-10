import React from 'react';
import type { StateHistoryEntry } from './incidentService';

interface Props { stateHistory: StateHistoryEntry[]; }

export const EscalationBadge: React.FC<Props> = ({ stateHistory }) => {
  const escalation = stateHistory.find(h => h.actor === 'system' && h.note?.includes('Escalated'));
  if (!escalation) return null;
  return (
    <span style={{ background: '#fef3c7', color: '#92400e', padding: '4px 12px', borderRadius: 12, fontSize: 13, fontWeight: 600 }}>
      Auto-escalated: {escalation.note}
    </span>
  );
};
