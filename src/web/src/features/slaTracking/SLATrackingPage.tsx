import { FC, useMemo, useState } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, Spinner, Stack, Text, Dropdown, IDropdownOption, Panel, PanelType } from '@fluentui/react';
import { useSLAStatus, useSLACompliance, useSLAHistory } from './hooks/useSLATracking';
import type { SLAStatusDto, SLABreachHistory } from './slaTrackingService';
const statusOpts: IDropdownOption[] = [{ key: '', text: 'All' }, { key: 'atRisk', text: 'At Risk' }, { key: 'breached', text: 'Breached' }, { key: 'met', text: 'Met' }, { key: 'onTrack', text: 'On Track' }];
const statusColor = (s: string) => s === 'Breached' ? 'red' : s === 'AtRisk' ? '#c19c00' : s === 'Met' ? 'green' : undefined;
const formatTime = (seconds?: number) => { if (seconds == null) return '-'; const abs = Math.abs(seconds); const mins = Math.floor(abs / 60); return seconds < 0 ? `${mins} min overdue` : `${mins} min remaining`; };
const SLATrackingPage: FC = () => {
  const [filter, setFilter] = useState('');
  const { data, isLoading, isError, refetch } = useSLAStatus(filter || undefined);
  const { data: compliance } = useSLACompliance('last7d');
  const [historyPipeline, setHistoryPipeline] = useState<string | null>(null);
  const historyData = useSLAHistory(historyPipeline ?? '');
  const items = useMemo(() => data ?? [], [data]);
  const columns: IColumn[] = useMemo(() => [
    { key: 'name', name: 'Pipeline', fieldName: 'pipelineName', minWidth: 180 },
    { key: 'bp', name: 'Business Plan', fieldName: 'businessPlan', minWidth: 120 },
    { key: 'status', name: 'SLA Status', minWidth: 80, onRender: (i: SLAStatusDto) => <Text styles={{ root: { color: statusColor(i.status), fontWeight: 600 } }}>{i.status}</Text> },
    { key: 'time', name: 'Time', minWidth: 120, onRender: (i: SLAStatusDto) => <Text styles={{ root: { color: (i.timeRemainingSeconds ?? 0) < 0 ? 'red' : '#c19c00' } }}>{formatTime(i.timeRemainingSeconds)}</Text> },
    { key: 'hist', name: '', minWidth: 80, onRender: (i: SLAStatusDto) => <button onClick={() => setHistoryPipeline(i.pipelineId)}>History</button> },
  ], []);
  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">SLA Tracking</Text>
      {compliance && <Stack horizontal tokens={{ childrenGap: 16 }}>{compliance.summary.map((s: { businessPlan: string; percentageMet: number; breachCount: number }) => <Stack key={s.businessPlan} styles={{ root: { padding: 12, border: '1px solid #e0e0e0', borderRadius: 4, minWidth: 160 } }}><Text variant="mediumPlus">{s.businessPlan}</Text><Text>Met: {s.percentageMet}%</Text><Text>Breaches: {s.breachCount}</Text></Stack>)}</Stack>}
      <Dropdown label="Status" options={statusOpts} selectedKey={filter} onChange={(_, o) => setFilter((o?.key as string) ?? '')} styles={{ root: { maxWidth: 200 } }} />
      {isLoading ? <Spinner /> : isError ? <Stack><Text>Error loading SLA data.</Text><button onClick={() => refetch()}>Retry</button></Stack> : items.length === 0 ? <Text>No SLA configurations found</Text> : <DetailsList items={items} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={i => i.pipelineId} />}
      <Panel isOpen={!!historyPipeline} onDismiss={() => setHistoryPipeline(null)} type={PanelType.medium} headerText="SLA Breach History">
        {historyData.isLoading ? <Spinner /> : historyData.data?.map((b: SLABreachHistory) => <Stack key={b.id} tokens={{ childrenGap: 4 }} styles={{ root: { padding: 8, borderBottom: '1px solid #e0e0e0' } }}><Text>Detected: {new Date(b.breachDetectedAt).toLocaleString()}</Text><Text>Window Closed: {new Date(b.slaWindowClosedAt).toLocaleString()}</Text>{b.completedAt && <Text>Completed: {new Date(b.completedAt).toLocaleString()}</Text>}{b.minutesOverdue != null && <Text>Overdue: {b.minutesOverdue} min</Text>}</Stack>)}
      </Panel>
    </Stack>
  );
};
export default SLATrackingPage;

