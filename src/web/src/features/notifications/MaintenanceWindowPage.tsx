import React, { useState } from 'react';
import { useMaintenanceWindows, useCreateMaintenanceWindow, useDeleteMaintenanceWindow } from './hooks/useNotifications';

const MaintenanceWindowPage: React.FC = () => {
  const { data: windows, isLoading } = useMaintenanceWindows();
  const createMut = useCreateMaintenanceWindow();
  const deleteMut = useDeleteMaintenanceWindow();
  const [name, setName] = useState('');
  const [start, setStart] = useState('');
  const [end, setEnd] = useState('');

  const handleCreate = () => { if (!name || !start || !end) return; createMut.mutate({ name, startTime: start, endTime: end, scopeType: 'All' }); setName(''); };
  const now = new Date();

  return (
    <div style={{ padding: 24 }}>
      <h2>Maintenance Windows</h2>
      <div style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
        <input value={name} onChange={e => setName(e.target.value)} placeholder='Window name' />
        <input type='datetime-local' value={start} onChange={e => setStart(e.target.value)} />
        <input type='datetime-local' value={end} onChange={e => setEnd(e.target.value)} />
        <button onClick={handleCreate} disabled={createMut.isPending}>Create</button>
      </div>
      {isLoading ? <p>Loading...</p> : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead><tr>{['Name','Start','End','Status','Actions'].map(h => <th key={h} style={{ textAlign: 'left', padding: 8, borderBottom: '2px solid #e5e7eb' }}>{h}</th>)}</tr></thead>
          <tbody>{windows?.map(w => {
            const s = new Date(w.startTime); const e = new Date(w.endTime);
            const status = now < s ? 'Scheduled' : now > e ? 'Expired' : 'Active';
            return (
              <tr key={w.id} style={{ borderBottom: '1px solid #f3f4f6' }}>
                <td style={{ padding: 8 }}>{w.name}</td>
                <td style={{ padding: 8 }}>{s.toLocaleString()}</td>
                <td style={{ padding: 8 }}>{e.toLocaleString()}</td>
                <td style={{ padding: 8 }}><span style={{ color: status === 'Active' ? '#16a34a' : status === 'Scheduled' ? '#2563eb' : '#9ca3af' }}>{status}</span></td>
                <td style={{ padding: 8 }}><button onClick={() => deleteMut.mutate(w.id)} disabled={deleteMut.isPending}>Delete</button></td>
              </tr>
            );
          })}</tbody>
        </table>
      )}
    </div>
  );
};
export default MaintenanceWindowPage;
