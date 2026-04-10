import { FC, useMemo } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, DefaultButton, Spinner, Stack, Text, MessageBar, MessageBarType } from '@fluentui/react';
import { usePipelines, useActivatePipeline, useDeactivatePipeline } from './hooks/usePipelineRegistrations';
import type { PipelineRegistrationResponse } from './pipelineTypes';

const PipelineRegistrationListPage: FC = () => {
  const { data, isLoading, isError } = usePipelines();
  const items = useMemo(() => data?.items ?? [], [data]);
  const columns: IColumn[] = useMemo(() => [
    { key: 'name', name: 'Pipeline', fieldName: 'pipelineName', minWidth: 150, isResizable: true },
    { key: 'layer', name: 'Layer', fieldName: 'medallionLayer', minWidth: 100 },
    { key: 'bp', name: 'Business Plan', fieldName: 'businessPlanName', minWidth: 140 },
    { key: 'active', name: 'Active', minWidth: 70, onRender: (i: PipelineRegistrationResponse) => <span>{i.isActive ? 'Yes' : 'No'}</span> },
    { key: 'actions', name: '', minWidth: 160, onRender: (i: PipelineRegistrationResponse) => <PipelineActions item={i} /> },
  ], []);
  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Pipelines</Text>
      {isError && <MessageBar messageBarType={MessageBarType.error}>Failed to load pipelines.</MessageBar>}
      {isLoading ? <Spinner label="Loading..." /> : <DetailsList items={items} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={(i) => i.id} />}
    </Stack>
  );
};
const PipelineActions: FC<{ item: PipelineRegistrationResponse }> = ({ item }) => {
  const act = useActivatePipeline(item.id);
  const deact = useDeactivatePipeline(item.id);
  return <Stack horizontal tokens={{ childrenGap: 8 }}>{item.isActive ? <DefaultButton text="Deactivate" onClick={() => deact.mutate()} /> : <DefaultButton text="Activate" onClick={() => act.mutate()} />}</Stack>;
};
export default PipelineRegistrationListPage;
