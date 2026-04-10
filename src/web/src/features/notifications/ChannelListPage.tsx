import React, { useState } from 'react';
import { useChannels, useCreateChannel, useDeleteChannel } from './hooks/useNotifications';

const ChannelListPage: React.FC = () => {
  const { data: channels, isLoading } = useChannels();
  const createMut = useCreateChannel();
  const deleteMut = useDeleteChannel();
  const [name, setName] = useState('');
  const [type, setType] = useState('Email');

  const handleCreate = () => { if (!name.trim()) return; createMut.mutate({ name, type, isEnabled: true }); setName(''); };

  return (
    <div style={{ padding: 24 }}>
      <h2>Notification Channels</h2>
      <div style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
        <input value={name} onChange={e => setName(e.target.value)} placeholder='Channel name' />
        <select value={type} onChange={e => setType(e.target.value)}><option>Email</option><option>Webhook</option></select>
        <button onClick={handleCreate} disabled={createMut.isPending}>Add Channel</button>
      </div>
      {isLoading ? <p>Loading...</p> : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead><tr>{['Name','Type','Enabled','Actions'].map(h => <th key={h} style={{ textAlign: 'left', padding: 8, borderBottom: '2px solid #e5e7eb' }}>{h}</th>)}</tr></thead>
          <tbody>{channels?.map(ch => (
            <tr key={ch.id} style={{ borderBottom: '1px solid #f3f4f6' }}>
              <td style={{ padding: 8 }}>{ch.name}</td>
              <td style={{ padding: 8 }}>{ch.type}</td>
              <td style={{ padding: 8 }}>{ch.isEnabled ? 'Yes' : 'No'}</td>
              <td style={{ padding: 8 }}><button onClick={() => deleteMut.mutate(ch.id)} disabled={deleteMut.isPending}>Delete</button></td>
            </tr>
          ))}</tbody>
        </table>
      )}
    </div>
  );
};
export default ChannelListPage;
