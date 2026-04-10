import { FC, useMemo } from 'react';
import { useParams } from 'react-router-dom';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, Spinner, Stack, Text } from '@fluentui/react';
import { useConnectorLogs } from './hooks/useConnectors';
import type { ConnectorLogEntry } from './connectorTypes';
const ConnectorExecutionLogViewer: FC = () => {
  const { id } = useParams<{ id: string }>();
  const { data, isLoading } = useConnectorLogs(id);
  const items = useMemo(() => data?.entries ?? [], [data]);
  const columns: IColumn[] = useMemo(() => [
    { key: 'at', name: 'Executed At', minWidth: 180, onRender: (i: ConnectorLogEntry) => <span>{new Date(i.executedAt).toLocaleString()}</span> },
    { key: 'status', name: 'Status', minWidth: 80, onRender: (i: ConnectorLogEntry) => <Text styles={{ root: { color: i.status === 'Failed' ? 'red' : 'green' } }}>{i.status}</Text> },
    { key: 'events', name: 'Events', fieldName: 'eventsProduced', minWidth: 70 },
    { key: 'duration', name: 'Duration (ms)', fieldName: 'durationMs', minWidth: 100 },
    { key: 'error', name: 'Error', fieldName: 'errorMessage', minWidth: 200 },
  ], []);
  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Execution Logs</Text>
      {data && <Text>Success Rate (last 30): {data.successRateLast30Cycles}%</Text>}
      {isLoading ? <Spinner /> : <DetailsList items={items} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={i => i.id} />}
    </Stack>
  );
};
export default ConnectorExecutionLogViewer;
