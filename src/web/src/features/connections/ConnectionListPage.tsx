import { FC, useMemo } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, DefaultButton, Spinner, Stack, Text, MessageBar, MessageBarType } from '@fluentui/react';
import { useConnections, useDeleteConnection, useTestConnection } from './hooks/useConnections';
import type { ConnectionResponse } from './connectionTypes';
const ConnectionListPage: FC = () => {
  const { data, isLoading, isError } = useConnections();
  const items = useMemo(() => data?.items ?? [], [data]);
  const columns: IColumn[] = useMemo(() => [
    { key: 'name', name: 'Connection', fieldName: 'connectionName', minWidth: 160, isResizable: true },
    { key: 'type', name: 'Type', fieldName: 'connectorTypeId', minWidth: 120 },
    { key: 'result', name: 'Last Test', fieldName: 'lastTestResult', minWidth: 100 },
    { key: 'actions', name: '', minWidth: 200, onRender: (i: ConnectionResponse) => <ConnActions item={i} /> },
  ], []);
  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Connections</Text>
      {isError && <MessageBar messageBarType={MessageBarType.error}>Failed to load.</MessageBar>}
      {isLoading ? <Spinner /> : <DetailsList items={items} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={i => i.id} />}
    </Stack>
  );
};
const ConnActions: FC<{ item: ConnectionResponse }> = ({ item }) => {
  const test = useTestConnection();
  const del = useDeleteConnection();
  return (<Stack horizontal tokens={{ childrenGap: 8 }}>
    <DefaultButton text="Test" onClick={() => test.mutate(item.id)} disabled={test.isPending} />
    <DefaultButton text="Delete" onClick={() => del.mutate(item.id)} disabled={del.isPending} />
    {test.data && <Text styles={{ root: { color: test.data.success ? 'green' : 'red' } }}>{test.data.success ? 'Passed' : test.data.message}</Text>}
  </Stack>);
};
export default ConnectionListPage;
