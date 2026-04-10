import React, { useState } from 'react';
import { useAddNote } from './hooks/useIncidents';

interface Props { incidentId: string; incidentState: string; }

export const NoteComposer: React.FC<Props> = ({ incidentId, incidentState }) => {
  const [content, setContent] = useState('');
  const mutation = useAddNote();
  const disabled = incidentState === 'Resolved' || incidentState === 'Closed';

  const handleSubmit = () => {
    if (!content.trim()) return;
    mutation.mutate({ id: incidentId, content }, { onSuccess: () => setContent('') });
  };

  return (
    <div style={{ marginTop: 16 }} title={disabled ? 'Notes cannot be added to resolved or closed incidents' : undefined}>
      <textarea value={content} onChange={e => setContent(e.target.value)} disabled={disabled}
        placeholder={disabled ? 'Notes cannot be added to resolved or closed incidents' : 'Add a note...'} rows={3} style={{ width: '100%' }} />
      <button onClick={handleSubmit} disabled={disabled || !content.trim() || mutation.isPending} style={{ marginTop: 4 }}>Submit Note</button>
    </div>
  );
};
