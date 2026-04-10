import React, { useState } from 'react';
import { useDeliveryLogs } from './hooks/useNotifications';

const statusColor: Record<string, string> = { Delivered: '#16a34a', Failed: '#dc2626', PermanentlyFailed: '#dc2626', Suppressed: '#ca8a04', Retrying: '#2563eb' };

const DeliveryLogPage: React.FC = () => {
  const [status, setStatus] = useState<string | undefined>();
  const { data, isLoading } = useDeliveryLogs({ status, limit: 100 });

  return (
    <div style={{ padding: 24 }}>
      <h2>Delivery Log</h2>
      <select value={status || ''} onChange={e => setStatus(e.target.value || undefined)} style={{ marginBottom: 16 }}>
        <option value=''>All Statuses</option>
        {['delivered','failed','permanentlyFailed','suppressed','retrying'].map(s => <option key={s} value={s}>{s}</option>)}
      </select>
      {isLoading ? <p>Loading...</p> : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead><tr>{['Event','Channel','Type','Recipient','Status','Attempts','Sent','Error'].map(h => <th key={h} style={{ textAlign: 'left', padding: 8, borderBottom: '2px solid #e5e7eb', fontSize: 13 }}>{h}</th>)}</tr></thead>
          <tbody>{data?.items.map(l => (
            <tr key={l.id} style={{ borderBottom: '1px solid #f3f4f6' }}>
              <td style={{ padding: 6, fontSize: 13 }}>{l.eventId.slice(0, 8)}</td>
              <td style={{ padding: 6, fontSize: 13 }}>{l.channelName}</td>
              <td style={{ padding: 6, fontSize: 13 }}>{l.channelType}</td>
              <td style={{ padding: 6, fontSize: 13 }}>{l.recipient}</td>
              <td style={{ padding: 6, fontSize: 13, color: statusColor[l.deliveryStatus] || '#6b7280' }}>{l.deliveryStatus}</td>
              <td style={{ padding: 6, fontSize: 13 }}>{l.attemptCount}</td>
              <td style={{ padding: 6, fontSize: 13 }}>{l.sentAt ? new Date(l.sentAt).toLocaleString() : '-'}</td>
              <td style={{ padding: 6, fontSize: 13 }}>{l.errorMessage || '-'}</td>
            </tr>
          ))}</tbody>
        </table>
      )}
    </div>
  );
};
export default DeliveryLogPage;
