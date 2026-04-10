import React, { useState } from 'react';
import { useTransitionState } from './hooks/useIncidents';
import type { IncidentDetail } from './incidentService';

const transitions: Record<string, string[]> = {
  Open: ['InProgress'], InProgress: ['Resolved'], Resolved: ['Closed'], Closed: [],
};

export const StateTransitionPanel: React.FC<{ incident: IncidentDetail }> = ({ incident }) => {
  const mutation = useTransitionState();
  const [note, setNote] = useState('');
  const [showResolve, setShowResolve] = useState(false);
  const validNext = transitions[incident.state] || [];

  if (!validNext.length) return null;

  const handleTransition = (toState: string) => {
    if (toState === 'Resolved') { setShowResolve(true); return; }
    mutation.mutate({ id: incident.id, toState: toState.charAt(0).toLowerCase() + toState.slice(1), etag: incident.etag });
  };

  const handleResolve = () => {
    if (note.length < 20) return;
    mutation.mutate({ id: incident.id, toState: 'resolved', resolutionNote: note, etag: incident.etag });
    setShowResolve(false);
  };

  return (
    <div style={{ marginTop: 16 }}>
      {validNext.map(s => <button key={s} onClick={() => handleTransition(s)} disabled={mutation.isPending} style={{ marginRight: 8 }}>
        {s === 'InProgress' ? 'Mark In Progress' : s === 'Resolved' ? 'Resolve' : 'Close'}
      </button>)}
      {showResolve && (
        <div style={{ marginTop: 12 }}>
          <textarea value={note} onChange={e => setNote(e.target.value)} placeholder='Resolution note (min 20 chars)' rows={3} style={{ width: '100%' }} />
          {note.length > 0 && note.length < 20 && <p style={{ color: '#dc2626' }}>Resolution note must be at least 20 characters</p>}
          <button onClick={handleResolve} disabled={note.length < 20 || mutation.isPending}>Confirm Resolve</button>
        </div>
      )}
    </div>
  );
};
