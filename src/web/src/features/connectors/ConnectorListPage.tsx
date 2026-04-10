import { FC, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, DefaultButton, Spinner, Stack, Text, Toggle } from '@fluentui/react';
import { useConnectors, useEnableConnector, useDisableConnector } from './hooks/useConnectors';
import type { ConnectorResponse } from './connectorTypes';
const ConnectorListPage: FC = () => {
  const navigate = useNavigate();
  const { data, isLoading } = useConnectors();
  const items = useMemo(() => data?.items ?? [], [data]);
  const columns: IColumn[] = useMemo(() => [
    { key: 'name', name: 'Connector', fieldName: 'connectorName', minWidth: 160, isResizable: true },
    { key: 'type', name: 'Type', fieldName: 'connectorTypeId', minWidth: 120 },
    { key: 'mode', name: 'Mode', fieldName: 'integrationMode', minWidth: 80 },
    { key: 'enabled', name: 'Enabled', minWidth: 80, onRender: (i: ConnectorResponse) => <EnableToggle item={i} /> },
    { key: 'logs', name: '', minWidth: 100, onRender: (i: ConnectorResponse) => <DefaultButton text="View Logs" onClick={() => navigate(`/admin/connectors/${i.id}/logs`)} /> },
  ], [navigate]);
  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Connectors</Text>
      {isLoading ? <Spinner /> : <DetailsList items={items} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={i => i.id} />}
    </Stack>
  );
};
const EnableToggle: FC<{ item: ConnectorResponse }> = ({ item }) => {
  const enable = useEnableConnector(item.id);
  const disable = useDisableConnector(item.id);
  return <Toggle checked={item.isEnabled} onChange={(_, checked) => { if (checked) enable.mutate(); else disable.mutate(); }} />;
};
export default ConnectorListPage;
