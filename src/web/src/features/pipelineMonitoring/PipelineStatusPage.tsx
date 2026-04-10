import { FC, useMemo, useState } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, Spinner, Stack, Text, Dropdown, IDropdownOption, Pivot, PivotItem, Panel, PanelType } from '@fluentui/react';
import { usePipelineStatus, useHopDetail, useMemSQLInterfaces } from './hooks/usePipelineMonitoring';
import type { PipelineStatusDto, HopSummaryDto, MemSQLInterfaceDto } from './pipelineMonitoringService';
const statusOpts: IDropdownOption[] = [{ key: '', text: 'All' }, { key: 'failed', text: 'Failed' }, { key: 'successful', text: 'Successful' }, { key: 'running', text: 'Running' }];
const statusColor = (s: string) => s === 'failed' ? 'red' : s === 'warning' ? '#c19c00' : s === 'successful' ? 'green' : 'grey';
const HopBadge: FC<{ hop: HopSummaryDto; onClick?: () => void }> = ({ hop, onClick }) => <span onClick={hop.hasDetail ? onClick : undefined} style={{ padding: '2px 8px', borderRadius: 4, backgroundColor: statusColor(hop.status), color: '#fff', cursor: hop.hasDetail ? 'pointer' : 'default', marginRight: 4, fontSize: 12 }}>{hop.layer}</span>;
const PipelineStatusPage: FC = () => {
  const [status, setStatus] = useState('');
  const [hopPanel, setHopPanel] = useState<{ executionId: string; layer: string } | null>(null);
  const { data, isLoading, isError, refetch } = usePipelineStatus(status || undefined);
  const { data: memData, isLoading: memLoading } = useMemSQLInterfaces();
  const hopDetail = useHopDetail(hopPanel?.executionId ?? '', hopPanel?.layer ?? '');
  const items = useMemo(() => data?.data ?? [], [data]);
  const columns: IColumn[] = useMemo(() => [
    { key: 'name', name: 'Pipeline', fieldName: 'pipelineName', minWidth: 180, isResizable: true },
    { key: 'bp', name: 'Business Plan', fieldName: 'businessPlan', minWidth: 120 },
    { key: 'status', name: 'Status', fieldName: 'status', minWidth: 80, onRender: (i: PipelineStatusDto) => <Text styles={{ root: { color: statusColor(i.status), fontWeight: 600 } }}>{i.status}</Text> },
    { key: 'lastRun', name: 'Last Run', minWidth: 160, onRender: (i: PipelineStatusDto) => <Text>{i.lastRunAt ? new Date(i.lastRunAt).toLocaleString() : '-'}</Text> },
    { key: 'hops', name: 'Hops', minWidth: 200, onRender: (i: PipelineStatusDto) => <>{i.hops.map((h, idx) => <HopBadge key={idx} hop={h} onClick={() => i.latestExecutionId && setHopPanel({ executionId: i.latestExecutionId, layer: h.layer })} />)}</> },
  ], []);
  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Pipeline Monitoring</Text>
      <Pivot>
        <PivotItem headerText="Pipelines">
          <Stack tokens={{ childrenGap: 8, padding: '8px 0' }}>
            <Dropdown label="Status" options={statusOpts} selectedKey={status} onChange={(_, o) => setStatus((o?.key as string) ?? '')} styles={{ root: { maxWidth: 200 } }} />
            {isLoading ? <Spinner /> : isError ? <Stack><Text>Error loading pipelines.</Text><button onClick={() => refetch()}>Retry</button></Stack> : items.length === 0 ? <Text>No pipelines configured</Text> : <DetailsList items={items} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={i => i.pipelineId} />}
          </Stack>
        </PivotItem>
        <PivotItem headerText="MemSQL Interfaces">
          {memLoading ? <Spinner /> : <DetailsList items={memData ?? []} columns={[
            { key: 'name', name: 'Interface', fieldName: 'interfaceName', minWidth: 150 },
            { key: 'status', name: 'Status', minWidth: 80, onRender: (i: MemSQLInterfaceDto) => <Text styles={{ root: { color: i.status === 'failed' ? 'red' : i.status === 'pending' ? '#c19c00' : undefined } }}>{i.status}</Text> },
            { key: 'pending', name: 'Pending', fieldName: 'pendingRecordCount', minWidth: 80 },
          ]} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={i => i.interfaceName} />}
        </PivotItem>
      </Pivot>
      <Panel isOpen={!!hopPanel} onDismiss={() => setHopPanel(null)} type={PanelType.medium} headerText="Hop Detail">
        {hopDetail.isLoading ? <Spinner /> : hopDetail.data ? <Stack tokens={{ childrenGap: 8 }}><Text>Layer: {hopDetail.data.layer}</Text><Text>Status: {hopDetail.data.status}</Text><Text>Duration: {hopDetail.data.durationSeconds}s</Text>{hopDetail.data.errorMessage && <Text styles={{ root: { color: 'red' } }}>Error: {hopDetail.data.errorMessage}</Text>}</Stack> : <Text>Not found</Text>}
      </Panel>
    </Stack>
  );
};
export default PipelineStatusPage;
