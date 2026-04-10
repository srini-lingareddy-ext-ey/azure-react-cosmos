import React, { useState, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useIncidentList } from './hooks/useIncidents';
import { IncidentFilterBar } from './IncidentFilterBar';
import type { IncidentFilters } from './incidentService';

const severityColor: Record<string, string> = {
  critical: '#dc2626', high: '#ea580c', medium: '#ca8a04', low: '#2563eb',
};

const IncidentListPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const [filters, setFiltersState] = useState<IncidentFilters>(() => ({
    severity: searchParams.get('severity') || undefined,
    state: searchParams.get('state') || undefined,
    from: searchParams.get('from') || undefined,
    to: searchParams.get('to') || undefined,
    limit: 50, offset: 0, sort: 'createdAt', order: 'desc',
  }));
  const setFilters = useCallback((f: IncidentFilters) => {
    setFiltersState(f);
    const params = new URLSearchParams();
    if (f.severity) params.set('severity', f.severity);
    if (f.state) params.set('state', f.state);
    if (f.from) params.set('from', f.from);
    if (f.to) params.set('to', f.to);
    setSearchParams(params, { replace: true });
  }, [setSearchParams]);
  const { data, isLoading, isError, refetch } = useIncidentList(filters);

  if (isError) return <div style={{ padding: 24 }}><p>Failed to load incidents.</p><button onClick={() => refetch()}>Retry</button></div>;

  return (
    <div style={{ padding: 24 }}>
      <h2>Incidents</h2>
      <IncidentFilterBar filters={filters} onChange={setFilters} />
      {isLoading ? <p>Loading...</p> : !data?.items.length ? <p>No incidents found</p> : (
        <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: 16 }}>
          <thead>
            <tr>{['Display ID','Severity','State','Monitor','Business Plan','ServiceNow','Created'].map(h => <th key={h} style={{ textAlign: 'left', padding: 8, borderBottom: '2px solid #e5e7eb' }}>{h}</th>)}</tr>
          </thead>
          <tbody>
            {data.items.map(i => (
              <tr key={i.id} onClick={() => navigate(`/incidents/${i.id}`)} style={{ cursor: 'pointer', borderBottom: '1px solid #f3f4f6' }}>
                <td style={{ padding: 8, borderLeft: `4px solid ${severityColor[i.severity.toLowerCase()] || '#9ca3af'}` }}>{i.displayId}</td>
                <td style={{ padding: 8 }}>{i.severity}</td>
                <td style={{ padding: 8 }}>{i.state}</td>
                <td style={{ padding: 8 }}>{i.monitorName}</td>
                <td style={{ padding: 8 }}>{i.businessPlan}</td>
                <td style={{ padding: 8 }}>{i.serviceNowTicketNumber || (i.ticketCreationStatus === 'Failed' ? <span style={{ color: '#dc2626' }}>Ticket failed</span> : '-')}</td>
                <td style={{ padding: 8 }}>{i.createdAt ? new Date(i.createdAt).toLocaleString() : '-'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      {data?.pagination && <p style={{ marginTop: 8 }}>Total: {data.pagination.total}{data.pagination.hasMore ? ' (more available)' : ''}</p>}
    </div>
  );
};

export default IncidentListPage;
