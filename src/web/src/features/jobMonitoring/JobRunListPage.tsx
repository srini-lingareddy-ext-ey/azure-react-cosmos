import { FC, useMemo, useState } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, Spinner, Stack, Text, Panel, PanelType, DefaultButton, Pivot, PivotItem, MessageBar, MessageBarType } from '@fluentui/react';
import { useJobsByExecution, useJobHistory } from './hooks/useJobMonitoring';
import type { JobRunDto } from './jobMonitoringService';
const statusColor = (s: string) => s === 'failed' ? 'red' : s === 'running' ? 'blue' : s === 'successful' ? 'green' : 'grey';
const JobRunListPage: FC<{ executionId: string; pipelineId?: string }> = ({ executionId, pipelineId }) => {
  const { data, isLoading, isError, refetch } = useJobsByExecution(executionId);
  const [selected, setSelected] = useState<JobRunDto | null>(null);
  const [historyJob, setHistoryJob] = useState<string | null>(null);
  const [historyDays, setHistoryDays] = useState(30);
  const history = useJobHistory(pipelineId ?? '', historyJob ?? '', historyDays);
  const items = useMemo(() => data ?? [], [data]);
  const columns: IColumn[] = useMemo(() => [
    { key: 'name', name: 'Job', fieldName: 'jobName', minWidth: 150, isResizable: true },
    { key: 'status', name: 'Status', minWidth: 80, onRender: (i: JobRunDto) => <Text styles={{ root: { color: statusColor(i.status), fontWeight: 600 } }}>{i.status}</Text> },
    { key: 'dur', name: 'Duration', minWidth: 80, onRender: (i: JobRunDto) => <Text>{i.durationSeconds ? `${i.durationSeconds}s` : '-'}</Text> },
    { key: 'retry', name: 'Retries', fieldName: 'retryCount', minWidth: 60 },
    { key: 'long', name: 'Long Run', minWidth: 60, onRender: (i: JobRunDto) => i.isLongRunning ? <Text styles={{ root: { color: '#c19c00', fontWeight: 600 } }}>Yes</Text> : null },
    { key: 'actions', name: '', minWidth: 140, onRender: (i: JobRunDto) => <Stack horizontal tokens={{ childrenGap: 4 }}>{i.hasGranularData && <DefaultButton text="Detail" onClick={() => setSelected(i)} />}<DefaultButton text="History" onClick={() => setHistoryJob(i.jobName)} /></Stack> },
  ], []);
  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xLarge">Jobs for Execution {executionId}</Text>
      {isLoading ? <Spinner /> : isError ? <Stack><Text>Error loading jobs.</Text><button onClick={() => refetch()}>Retry</button></Stack> : items.length === 0 ? <MessageBar messageBarType={MessageBarType.info}>No jobs found</MessageBar> : <DetailsList items={items} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={i => i.jobName} />}
      <Panel isOpen={!!selected} onDismiss={() => setSelected(null)} type={PanelType.medium} headerText={selected?.jobName ?? 'Job Detail'}>
        {selected && <Stack tokens={{ childrenGap: 8 }}><Text>Status: {selected.status}</Text>{selected.errorMessage && <><Text variant="mediumPlus">Error</Text><pre style={{ fontFamily: 'monospace', whiteSpace: 'pre-wrap', background: '#f4f4f4', padding: 8, borderRadius: 4 }}>{selected.errorMessage}</pre></>}{selected.stackTrace && <><Text variant="mediumPlus">Stack Trace</Text><pre style={{ fontFamily: 'monospace', whiteSpace: 'pre-wrap', background: '#f4f4f4', padding: 8, borderRadius: 4 }}>{selected.stackTrace}</pre></>}{selected.sourceSystemUrl && <a href={selected.sourceSystemUrl} target="_blank" rel="noreferrer">View in source system</a>}</Stack>}
      </Panel>
      <Panel isOpen={!!historyJob} onDismiss={() => setHistoryJob(null)} type={PanelType.large} headerText={`History: ${historyJob}`}>
        <Pivot onLinkClick={(item) => setHistoryDays(item?.props.headerText === '90 Days' ? 90 : 30)}><PivotItem headerText="30 Days" /><PivotItem headerText="90 Days" /></Pivot>
        {history.isLoading ? <Spinner /> : history.data ? <Stack tokens={{ childrenGap: 8 }}><Text>Total: {history.data.summary.totalRuns} | Success Rate: {history.data.summary.successRate}% | Avg: {history.data.summary.averageDurationSeconds?.toFixed(1)}s</Text><DetailsList items={history.data.history} columns={[{ key: 's', name: 'Status', fieldName: 'status', minWidth: 80 }, { key: 'd', name: 'Duration', minWidth: 80, onRender: (i: JobRunDto) => <Text>{i.durationSeconds?.toFixed(1)}s</Text> }, { key: 't', name: 'Start', minWidth: 160, onRender: (i: JobRunDto) => <Text>{i.startTime ? new Date(i.startTime).toLocaleString() : '-'}</Text> }]} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={(_, i) => String(i)} /></Stack> : null}
      </Panel>
    </Stack>
  );
};
export default JobRunListPage;
