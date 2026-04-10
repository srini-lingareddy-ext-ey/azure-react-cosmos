import React, { useState } from 'react';
import { useRoutingRules, useCreateRoutingRule, useDeleteRoutingRule } from './hooks/useNotifications';

const RoutingRuleListPage: React.FC = () => {
  const { data: rules, isLoading } = useRoutingRules();
  const createMut = useCreateRoutingRule();
  const deleteMut = useDeleteRoutingRule();
  const [name, setName] = useState('');
  const [scopeType, setScopeType] = useState('All');

  const handleCreate = () => { if (!name.trim()) return; createMut.mutate({ name, isEnabled: true, scopeType }); setName(''); };

  return (
    <div style={{ padding: 24 }}>
      <h2>Routing Rules</h2>
      <div style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
        <input value={name} onChange={e => setName(e.target.value)} placeholder='Rule name' />
        <select value={scopeType} onChange={e => setScopeType(e.target.value)}><option>All</option><option>BusinessPlan</option><option>Monitor</option></select>
        <button onClick={handleCreate} disabled={createMut.isPending}>Add Rule</button>
      </div>
      {isLoading ? <p>Loading...</p> : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead><tr>{['Name','Scope','Classifications','Severities','Actions'].map(h => <th key={h} style={{ textAlign: 'left', padding: 8, borderBottom: '2px solid #e5e7eb' }}>{h}</th>)}</tr></thead>
          <tbody>{rules?.map(r => (
            <tr key={r.id} style={{ borderBottom: '1px solid #f3f4f6' }}>
              <td style={{ padding: 8 }}>{r.name}</td>
              <td style={{ padding: 8 }}>{r.scopeType === 'All' ? 'All' : `${r.scopeType}: ${r.scopeValue || '-'}`}</td>
              <td style={{ padding: 8 }}>{r.classifications.length ? r.classifications.join(', ') : 'All'}</td>
              <td style={{ padding: 8 }}>{r.severities.length ? r.severities.join(', ') : 'All'}</td>
              <td style={{ padding: 8 }}><button onClick={() => deleteMut.mutate(r.id)} disabled={deleteMut.isPending}>Delete</button></td>
            </tr>
          ))}</tbody>
        </table>
      )}
    </div>
  );
};
export default RoutingRuleListPage;
