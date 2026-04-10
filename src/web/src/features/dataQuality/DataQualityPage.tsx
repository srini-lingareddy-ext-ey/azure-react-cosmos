import { FC, useMemo, useState } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, Spinner, Stack, Text, Dropdown, IDropdownOption, Pivot, PivotItem, Panel, PanelType } from '@fluentui/react';
import { useDQStatus, useDQChecks } from './hooks/useDataQuality';
import type { DataQualityStatusDto, DataQualityCheck } from './dataQualityService';
const qsOpts: IDropdownOption[] = [{ key: '', text: 'All' }, { key: 'passing', text: 'Passing' }, { key: 'warning', text: 'Warning' }, { key: 'failing', text: 'Failing' }, { key: 'noData', text: 'No Data' }];
const qColor = (s: string) => s === 'Failing' ? 'red' : s === 'Warning' ? '#c19c00' : undefined;
const lColor = (s: string) => s === 'Stale' ? 'red' : s === 'Approaching' ? '#c19c00' : undefined;
const DataQualityPage: FC = () => {
  const [filter, setFilter] = useState('');
  const { data, isLoading, isError, refetch } = useDQStatus(filter || undefined);
  const [checksPanel, setChecksPanel] = useState<{ pipelineId: string; scoreId: string } | null>(null);
  const checks = useDQChecks(checksPanel?.pipelineId ?? '', checksPanel?.scoreId ?? '');
  const items = useMemo(() => data ?? [], [data]);
  const qColumns: IColumn[] = useMemo(() => [
    { key: 'name', name: 'Pipeline', fieldName: 'pipelineName', minWidth: 180 },
    { key: 'bp', name: 'Business Plan', fieldName: 'businessPlan', minWidth: 120 },
    { key: 'score', name: 'Score', minWidth: 60, onRender: (i: DataQualityStatusDto) => <Text>{i.qualityScore?.toFixed(1) ?? 'No data'}</Text> },
    { key: 'qs', name: 'Quality', minWidth: 80, onRender: (i: DataQualityStatusDto) => <Text styles={{ root: { color: qColor(i.qualityStatus), fontWeight: 600 } }}>{i.qualityStatus}</Text> },
  ], []);
  const lColumns: IColumn[] = useMemo(() => [
    { key: 'name', name: 'Pipeline', fieldName: 'pipelineName', minWidth: 180 },
    { key: 'last', name: 'Last Run', minWidth: 160, onRender: (i: DataQualityStatusDto) => <Text>{i.lastSuccessfulRunAt ? new Date(i.lastSuccessfulRunAt).toLocaleString() : 'No data'}</Text> },
    { key: 'ls', name: 'Latency', minWidth: 80, onRender: (i: DataQualityStatusDto) => <Text styles={{ root: { color: lColor(i.latencyStatus), fontWeight: 600 } }}>{i.latencyStatus}</Text> },
  ], []);
  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Data Quality & Latency</Text>
      <Dropdown label="Quality Status" options={qsOpts} selectedKey={filter} onChange={(_, o) => setFilter((o?.key as string) ?? '')} styles={{ root: { maxWidth: 200 } }} />
      <Pivot>
        <PivotItem headerText="Quality Scores">
          {isLoading ? <Spinner /> : isError ? <Stack><Text>Error loading data.</Text><button onClick={() => refetch()}>Retry</button></Stack> : items.length === 0 ? <Text>No data quality scores configured</Text> : <DetailsList items={items} columns={qColumns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={i => i.pipelineId} />}
        </PivotItem>
        <PivotItem headerText="Data Latency">
          {isLoading ? <Spinner /> : <DetailsList items={items} columns={lColumns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={i => i.pipelineId} />}
        </PivotItem>
      </Pivot>
      <Panel isOpen={!!checksPanel} onDismiss={() => setChecksPanel(null)} type={PanelType.medium} headerText="Quality Checks">
        {checks.isLoading ? <Spinner /> : checks.data?.map((c: DataQualityCheck, i: number) => <Stack key={i} tokens={{ childrenGap: 4 }} styles={{ root: { padding: 8, background: c.passed ? undefined : '#fde7e9', borderRadius: 4, marginBottom: 4 } }}><Text variant="mediumPlus">{c.checkName}</Text><Text>Passed: {String(c.passed)} | Failure Rate: {c.failureRate}% | Evaluated: {c.recordsEvaluated}</Text></Stack>)}
      </Panel>
    </Stack>
  );
};
export default DataQualityPage;
