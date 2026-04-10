import React from 'react';
import type { IncidentFilters } from './incidentService';

interface Props { filters: IncidentFilters; onChange: (f: IncidentFilters) => void; }

export const IncidentFilterBar: React.FC<Props> = ({ filters, onChange }) => {
  const update = (patch: Partial<IncidentFilters>) => onChange({ ...filters, ...patch, offset: 0 });
  return (
    <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap', marginBottom: 16 }}>
      <select value={filters.severity || ''} onChange={e => update({ severity: e.target.value || undefined })}>
        <option value=''>All Severities</option>
        {['critical','high','medium','low'].map(s => <option key={s} value={s}>{s}</option>)}
      </select>
      <select value={filters.state || ''} onChange={e => update({ state: e.target.value || undefined })}>
        <option value=''>All States</option>
        {['open','inProgress','resolved','closed'].map(s => <option key={s} value={s}>{s}</option>)}
      </select>
      <input type='date' value={filters.from || ''} onChange={e => update({ from: e.target.value || undefined })} placeholder='From' />
      <input type='date' value={filters.to || ''} onChange={e => update({ to: e.target.value || undefined })} placeholder='To' />
    </div>
  );
};
