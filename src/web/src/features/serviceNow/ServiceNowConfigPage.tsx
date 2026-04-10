import React, { useState, useEffect } from 'react';
import { useServiceNowConfig, useUpsertServiceNowConfig } from './hooks/useServiceNowConfig';

const ServiceNowConfigPage: React.FC = () => {
  const { data: config, isLoading } = useServiceNowConfig();
  const upsertMut = useUpsertServiceNowConfig();
  const [urgencyErrors, setUrgencyErrors] = useState<Record<string, string>>({});
  const [form, setForm] = useState({
    endpointUrl: '', authType: 'Basic',
    username: '', password: '', clientId: '', clientSecret: '',
    callerUserId: '', ticketTemplate: '',
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

  const validateUrgency = (): boolean => {
    const errors: Record<string, string> = {};
    for (const [k, v] of Object.entries(form.urgencyMapping)) {
      if (v < 1 || v > 4) errors[k] = 'Urgency must be between 1 and 4';
    }
    setUrgencyErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSave = () => {
    if (!validateUrgency()) return;
    const credentialSecretName = form.authType === 'Basic'
      ? (form.username || form.password ? `${form.username}:${form.password}` : undefined)
      : (form.clientId || form.clientSecret ? `${form.clientId}:${form.clientSecret}` : undefined);
    upsertMut.mutate({
      endpointUrl: form.endpointUrl, authType: form.authType,
      credentialSecretName: credentialSecretName || undefined,
      callerUserId: form.callerUserId || undefined, ticketTemplate: form.ticketTemplate || undefined,
      urgencyMapping: form.urgencyMapping, severityMapping: form.severityMapping, stateMapping: form.stateMapping,
    });
  };

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
      {form.authType === 'Basic' && (
        <>
          <label>Username <em>(write-only)</em></label>
          <input value={form.username} onChange={e => setForm(f => ({ ...f, username: e.target.value }))} placeholder='Enter to update' style={{ width: '100%', marginBottom: 12 }} />
          <label>Password <em>(write-only)</em></label>
          <input type='password' value={form.password} onChange={e => setForm(f => ({ ...f, password: e.target.value }))} placeholder='Enter to update' style={{ width: '100%', marginBottom: 12 }} />
        </>
      )}
      {form.authType === 'OAuth' && (
        <>
          <label>Client ID <em>(write-only)</em></label>
          <input value={form.clientId} onChange={e => setForm(f => ({ ...f, clientId: e.target.value }))} placeholder='Enter to update' style={{ width: '100%', marginBottom: 12 }} />
          <label>Client Secret <em>(write-only)</em></label>
          <input type='password' value={form.clientSecret} onChange={e => setForm(f => ({ ...f, clientSecret: e.target.value }))} placeholder='Enter to update' style={{ width: '100%', marginBottom: 12 }} />
        </>
      )}
      <label>Ticket Template</label>
      <textarea value={form.ticketTemplate} onChange={e => setForm(f => ({ ...f, ticketTemplate: e.target.value }))} rows={3} style={{ width: '100%', marginBottom: 12 }} />
      {form.ticketTemplate && <p style={{ color: '#6b7280', fontSize: 13 }}>Preview: {form.ticketTemplate.replace('{severity}', 'critical').replace('{monitorName}', 'Finance Monitor')}</p>}
      <h3 style={{ marginTop: 16 }}>Urgency Mapping</h3>
      {Object.entries(form.urgencyMapping).map(([k, v]) => (
        <div key={k} style={{ marginBottom: 4 }}>
          <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
            <span style={{ width: 80 }}>{k}</span>
            <input type='number' min={1} max={4} value={v} onChange={e => {
              const val = parseInt(e.target.value) || 0;
              setForm(f => ({ ...f, urgencyMapping: { ...f.urgencyMapping, [k]: val } }));
              if (val >= 1 && val <= 4) setUrgencyErrors(prev => { const next = { ...prev }; delete next[k]; return next; });
            }} />
          </div>
          {urgencyErrors[k] && <span style={{ color: '#dc2626', fontSize: 13 }}>{urgencyErrors[k]}</span>}
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
