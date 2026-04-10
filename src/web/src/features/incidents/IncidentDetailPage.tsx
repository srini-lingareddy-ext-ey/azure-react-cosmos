import React from 'react';
import { useParams, Link } from 'react-router-dom';
import { useIncidentDetail } from './hooks/useIncidents';
import { StateTransitionPanel } from './StateTransitionPanel';
import { NoteComposer } from './NoteComposer';
import { EscalationBadge } from './EscalationBadge';
import { RetryTicketButton } from './RetryTicketButton';

const IncidentDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { data: incident, isLoading, isError } = useIncidentDetail(id!);

  if (isLoading) return <p style={{ padding: 24 }}>Loading...</p>;
  if (isError || !incident) return <div style={{ padding: 24 }}><p>Incident not found.</p><Link to='/incidents'>Back</Link></div>;

  return (
    <div style={{ padding: 24 }}>
      <Link to='/incidents'>Back to list</Link>
      <h2>{incident.displayId} - {incident.monitorName}</h2>
      <EscalationBadge stateHistory={incident.stateHistory} />
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24, marginTop: 16 }}>
        <div>
          <h3>Summary</h3>
          <p><strong>Severity:</strong> {incident.severity}</p>
          <p><strong>State:</strong> {incident.state}</p>
          <p><strong>Business Plan:</strong> {incident.businessPlan}</p>
          <p><strong>Recurrence:</strong> {incident.recurrenceCount}</p>
          {incident.resolutionNote && <p><strong>Resolution:</strong> {incident.resolutionNote}</p>}
          {incident.affectedPipelineId && <p><strong>Pipeline:</strong> {incident.affectedPipelineId}</p>}
          <StateTransitionPanel incident={incident} />
        </div>
        <div>
          <h3>ServiceNow</h3>
          <p><strong>Ticket:</strong> {incident.serviceNow.ticketNumber ? <a href={incident.serviceNow.ticketUrl || '#'} target='_blank' rel='noreferrer'>{incident.serviceNow.ticketNumber}</a> : 'N/A'}</p>
          <p><strong>Status:</strong> {incident.serviceNow.ticketStatus || incident.serviceNow.ticketCreationStatus}</p>
          {incident.serviceNow.lastSyncedAt && <p><strong>Last Sync:</strong> {new Date(incident.serviceNow.lastSyncedAt).toLocaleString()}</p>}
          {incident.serviceNow.ticketCreationStatus === 'Failed' && <RetryTicketButton incidentId={incident.id} />}
        </div>
      </div>
      <h3 style={{ marginTop: 24 }}>Timeline</h3>
      <div>{incident.stateHistory.map((h, i) => (
        <div key={i} style={{ padding: 8, borderLeft: '2px solid #6366f1', marginBottom: 8, marginLeft: 8 }}>
          <strong>{h.fromState || 'new'} &rarr; {h.toState}</strong> by {h.actor} at {new Date(h.timestamp).toLocaleString()}
          {h.note && <p style={{ margin: '4px 0 0', color: '#6b7280' }}>{h.note}</p>}
        </div>
      ))}</div>
      <h3 style={{ marginTop: 24 }}>Notes</h3>
      {incident.notes.map(n => (
        <div key={n.noteId} style={{ padding: 8, background: '#f9fafb', borderRadius: 4, marginBottom: 8 }}>
          <strong>{n.authorId === 'servicenow' ? <span style={{ color: '#7c3aed' }}>ServiceNow</span> : n.authorName}</strong>
          <span style={{ marginLeft: 8, color: '#9ca3af' }}>{new Date(n.createdAt).toLocaleString()}</span>
          {n.syncedToServiceNow && <span style={{ marginLeft: 8, color: '#16a34a' }}>Synced</span>}
          <p style={{ margin: '4px 0 0' }}>{n.content}</p>
        </div>
      ))}
      <NoteComposer incidentId={incident.id} incidentState={incident.state} />
    </div>
  );
};

export default IncidentDetailPage;
