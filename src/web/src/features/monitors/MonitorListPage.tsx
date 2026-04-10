import { FC, useMemo, useState } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, DefaultButton, Spinner, Stack, Text, Dropdown, IDropdownOption } from '@fluentui/react';
import { useMonitors, usePauseMonitor, useActivateMonitor } from './hooks/useMonitors';
import type { MonitorResponse } from './monitorTypes';
const statusOpts: IDropdownOption[] = [{ key: '', text: 'All' }, { key: 'Active', text: 'Active' }, { key: 'Paused', text: 'Paused' }, { key: 'Error', text: 'Error' }];
const MonitorListPage: FC = () => {
  const [status, setStatus] = useState('');
  const { data, isLoading } = useMonitors(status || undefined);
  const items = useMemo(() => data?.items ?? [], [data]);
  const columns: IColumn[] = useMemo(() => [
    { key: 'name', name: 'Monitor', fieldName: 'monitorName', minWidth: 150, isResizable: true },
    { key: 'entity', name: 'Entity', fieldName: 'entityName', minWidth: 120 },
    { key: 'conn', name: 'Connection', fieldName: 'connectionName', minWidth: 120 },
    { key: 'status', name: 'Status', fieldName: 'status', minWidth: 80, onRender: (i: MonitorResponse) => <Text styles={{ root: { color: i.status === 'Error' ? 'red' : undefined, fontWeight: i.status === 'Error' ? 600 : undefined } }}>{i.status}</Text> },
    { key: 'actions', name: '', minWidth: 120, onRender: (i: MonitorResponse) => <MonActions item={i} /> },
  ], []);
  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Monitors</Text>
      <Dropdown label="Status" options={statusOpts} selectedKey={status} onChange={(_, o) => setStatus((o?.key as string) ?? '')} styles={{ root: { maxWidth: 200 } }} />
      {isLoading ? <Spinner /> : <DetailsList items={items} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={i => i.id} />}
    </Stack>
  );
};
const MonActions: FC<{ item: MonitorResponse }> = ({ item }) => {
  const pause = usePauseMonitor(item.id);
  const activate = useActivateMonitor(item.id);
  return <Stack horizontal tokens={{ childrenGap: 8 }}>{item.status === 'Active' ? <DefaultButton text="Pause" onClick={() => pause.mutate()} /> : <DefaultButton text="Activate" onClick={() => activate.mutate()} />}</Stack>;
};
export default MonitorListPage;
