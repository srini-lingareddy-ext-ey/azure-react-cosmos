import React, { useState, useEffect } from 'react';
import { useServiceNowConfig, useUpsertServiceNowConfig } from './hooks/useServiceNowConfig';

const ServiceNowConfigPage: React.FC = () => {
  const { data: config, isLoading } = useServiceNowConfig();
  const upsertMut = useUpsertServiceNowConfig();
  const [form, setForm] = useState({
    endpointUrl: '', authType: 'Basic', credentialSecretName: '', callerUserId: '', ticketTemplate: '',
    urgencyMapping: { critical: 1, high: 2, medium: 3, low: 4 } as Record<string, number>,
    severityMapping: { critical: 'critical', high: 'high', medium: 'medium' } as Record<string, string>,
    stateMapping: { open: 'New', inProgress: 'In Progress', resolved: 'Resolved', closed: 'Closed' } as Record<string, string>,
  });

  useEffect(() => {
    if (config) setForm(f => ({
      ...f, endpointUrl: config.endpointUrl, authType: config.authType,
      callerUserId: config.callerUserId || '', ticketTemplate: config.ticketTemplate || '',
      urgencyMapping: config.urgencyMapping || f.urgencyMapping,
      severityMapping: config.severityMapping || f.severityMapping,
      stateMapping: config.stateMapping || f.stateMapping,
    }));
  }, [config]);

  const handleSave = () => upsertMut.mutate({
    endpointUrl: form.endpointUrl, authType: form.authType,
    credentialSecretName: form.credentialSecretName || undefined,
    callerUserId: form.callerUserId || undefined, ticketTemplate: form.ticketTemplate || undefined,
    urgencyMapping: form.urgencyMapping, severityMapping: form.severityMapping, stateMapping: form.stateMapping,
  });

  if (isLoading) return <p style={{ padding: 24 }}>Loading...</p>;

  return (
    <div style={{ padding: 24, maxWidth: 700 }}>
      <h2>ServiceNow Integration</h2>
      <label>Endpoint URL</label>
      <input value={form.endpointUrl} onChange={e => setForm(f => ({ ...f, endpointUrl: e.target.value }))} style={{ width: '100%', marginBottom: 12 }} />
      <label>Auth Type</label>
      <select value={form.authType} onChange={e => setForm(f => ({ ...f, authType: e.target.value }))} style={{ width: '100%', marginBottom: 12 }}>
        <option value='Basic'>Basic</option><option value='OAuth'>OAuth</option>
      </select>
      <label>Credential Secret Name <em>(write-only)</em></label>
      <input value={form.credentialSecretName} onChange={e => setForm(f => ({ ...f, credentialSecretName: e.target.value }))} placeholder='Enter to update' style={{ width: '100%', marginBottom: 12 }} />
      <label>Ticket Template</label>
      <textarea value={form.ticketTemplate} onChange={e => setForm(f => ({ ...f, ticketTemplate: e.target.value }))} rows={3} style={{ width: '100%', marginBottom: 12 }} />
      {form.ticketTemplate && <p style={{ color: '#6b7280', fontSize: 13 }}>Preview: {form.ticketTemplate.replace('{severity}', 'critical').replace('{monitorName}', 'Finance Monitor')}</p>}
      <h3 style={{ marginTop: 16 }}>Urgency Mapping</h3>
      {Object.entries(form.urgencyMapping).map(([k, v]) => (
        <div key={k} style={{ display: 'flex', gap: 8, marginBottom: 4 }}>
          <span style={{ width: 80 }}>{k}</span>
          <input type='number' min={1} max={4} value={v} onChange={e => setForm(f => ({ ...f, urgencyMapping: { ...f.urgencyMapping, [k]: parseInt(e.target.value) || 1 } }))} />
        </div>
      ))}
      <h3 style={{ marginTop: 16 }}>State Mapping</h3>
      {Object.entries(form.stateMapping).map(([k, v]) => (
        <div key={k} style={{ display: 'flex', gap: 8, marginBottom: 4 }}>
          <span style={{ width: 100 }}>{k}</span>
          <input value={v} onChange={e => setForm(f => ({ ...f, stateMapping: { ...f.stateMapping, [k]: e.target.value } }))} />
        </div>
      ))}
      <button onClick={handleSave} disabled={upsertMut.isPending} style={{ marginTop: 16 }}>{upsertMut.isPending ? 'Saving...' : 'Save Configuration'}</button>
      {upsertMut.isSuccess && <p style={{ color: '#16a34a', marginTop: 8 }}>Configuration saved.</p>}
    </div>
  );
};
export default ServiceNowConfigPage;
