import { FC, useMemo, useState } from 'react';
import {
  DetailsList,
  IColumn,
  SelectionMode,
  CheckboxVisibility,
  Spinner,
  Stack,
  Text,
  Dropdown,
  IDropdownOption,
} from '@fluentui/react';
import { useSLAStatus } from './hooks/useSLA';
import type { SLAStatusDto } from './slaService';

const statusOpts: IDropdownOption[] = [
  { key: '', text: 'All' },
  { key: 'atRisk', text: 'At Risk' },
  { key: 'breached', text: 'Breached' },
  { key: 'met', text: 'Met' },
  { key: 'onTrack', text: 'On Track' },
];

const statusColor = (s: string): string | undefined => {
  const lower = s.toLowerCase();
  if (lower === 'breached') return 'red';
  if (lower === 'atrisk') return '#c19c00';
  if (lower === 'met') return 'green';
  return undefined;
};

const formatTime = (status: string, seconds?: number, completedAt?: string): string => {
  if (status.toLowerCase() === 'met' && completedAt) {
    return new Date(completedAt).toLocaleTimeString();
  }
  if (seconds == null) return '-';
  const abs = Math.abs(seconds);
  const mins = Math.floor(abs / 60);
  return seconds < 0 ? `${mins} min overdue` : `${mins} min remaining`;
};

const timeColor = (status: string, seconds?: number): string | undefined => {
  if (status.toLowerCase() === 'met') return 'green';
  if (seconds != null && seconds < 0) return 'red';
  if (status.toLowerCase() === 'atrisk') return '#c19c00';
  return undefined;
};

interface SLAStatusTableProps {
  onSelectPipeline?: (pipelineId: string) => void;
}

const SLAStatusTable: FC<SLAStatusTableProps> = ({ onSelectPipeline }) => {
  const [filter, setFilter] = useState('');
  const { data, isLoading, isError, refetch } = useSLAStatus(filter || undefined);
  const items = useMemo(() => data ?? [], [data]);

  const columns: IColumn[] = useMemo(
    () => [
      { key: 'name', name: 'Pipeline', fieldName: 'pipelineName', minWidth: 180 },
      { key: 'bp', name: 'Business Plan', fieldName: 'businessPlan', minWidth: 120 },
      {
        key: 'status',
        name: 'SLA Status',
        minWidth: 80,
        onRender: (i: SLAStatusDto) => (
          <Text styles={{ root: { color: statusColor(i.status), fontWeight: 600 } }}>
            {i.status}
          </Text>
        ),
      },
      {
        key: 'time',
        name: 'Time',
        minWidth: 140,
        onRender: (i: SLAStatusDto) => (
          <Text styles={{ root: { color: timeColor(i.status, i.timeRemainingSeconds) } }}>
            {formatTime(i.status, i.timeRemainingSeconds, i.completedAt)}
          </Text>
        ),
      },
      {
        key: 'hist',
        name: '',
        minWidth: 80,
        onRender: (i: SLAStatusDto) => (
          <button onClick={() => onSelectPipeline?.(i.pipelineId)}>History</button>
        ),
      },
    ],
    [onSelectPipeline]
  );

  return (
    <Stack tokens={{ childrenGap: 12 }}>
      <Dropdown
        label="Status"
        options={statusOpts}
        selectedKey={filter}
        onChange={(_, o) => setFilter((o?.key as string) ?? '')}
        styles={{ root: { maxWidth: 200 } }}
      />
      {isLoading ? (
        <Spinner label="Loading SLA status..." />
      ) : isError ? (
        <Stack horizontal tokens={{ childrenGap: 8 }}>
          <Text>Error loading SLA data.</Text>
          <button onClick={() => refetch()}>Retry</button>
        </Stack>
      ) : items.length === 0 ? (
        <Text>No SLA configurations found</Text>
      ) : (
        <DetailsList
          items={items}
          columns={columns}
          selectionMode={SelectionMode.none}
          checkboxVisibility={CheckboxVisibility.hidden}
          getKey={(i: SLAStatusDto) => i.pipelineId}
        />
      )}
    </Stack>
  );
};

export default SLAStatusTable;
