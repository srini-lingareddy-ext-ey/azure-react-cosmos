import React from 'react';
import { useRetryTicket } from './hooks/useIncidents';

export const RetryTicketButton: React.FC<{ incidentId: string }> = ({ incidentId }) => {
  const mutation = useRetryTicket();
  return (
    <div style={{ marginTop: 8 }}>
      <button onClick={() => mutation.mutate(incidentId)} disabled={mutation.isPending} style={{ color: '#dc2626' }}>
        {mutation.isPending ? 'Retrying...' : 'Retry ticket creation'}
      </button>
      {mutation.isError && <p style={{ color: '#dc2626', fontSize: 13 }}>Retry failed. Please try again.</p>}
      {mutation.isSuccess && <p style={{ color: '#16a34a', fontSize: 13 }}>Ticket created successfully.</p>}
    </div>
  );
};
