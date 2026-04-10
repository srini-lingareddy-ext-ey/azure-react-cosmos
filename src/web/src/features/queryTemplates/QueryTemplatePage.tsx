import { FC, useMemo } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, Spinner, Stack, Text } from '@fluentui/react';
import { useQueryTemplates } from './hooks/useQueryTemplates';
import type { QueryTemplateResponse } from './queryTemplateTypes';
const QueryTemplatePage: FC = () => {
  const { data, isLoading } = useQueryTemplates();
  const items = useMemo(() => data?.items ?? [], [data]);
  const columns: IColumn[] = useMemo(() => [
    { key: 'name', name: 'Template', fieldName: 'templateName', minWidth: 160, isResizable: true },
    { key: 'type', name: 'Connector Type', fieldName: 'connectorTypeId', minWidth: 120 },
    { key: 'active', name: 'Active', minWidth: 70, onRender: (i: QueryTemplateResponse) => <span>{i.isActive ? 'Yes' : 'No'}</span> },
  ], []);
  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Query Templates</Text>
      {isLoading ? <Spinner /> : <DetailsList items={items} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={i => i.id} />}
    </Stack>
  );
};
export default QueryTemplatePage;
