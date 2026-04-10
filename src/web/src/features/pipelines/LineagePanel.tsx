import { FC } from 'react';
import { Panel, Stack, Text, DefaultButton, Spinner } from '@fluentui/react';
import { useLineage, useDeleteLineage } from './hooks/useLineage';
interface Props { pipelineId: string; isOpen: boolean; onDismiss: () => void; }
const LineagePanel: FC<Props> = ({ pipelineId, isOpen, onDismiss }) => {
  const { data, isLoading } = useLineage(isOpen ? pipelineId : undefined);
  const deleteMut = useDeleteLineage(pipelineId);
  return (
    <Panel isOpen={isOpen} onDismiss={onDismiss} headerText="Pipeline Lineage" isLightDismiss>
      {isLoading ? <Spinner /> : (
        <Stack tokens={{ childrenGap: 12 }}>
          <Text variant="large">Upstream</Text>
          {data?.upstream.map(e => <Stack horizontal key={e.relationshipId} horizontalAlign="space-between"><Text>{e.relatedPipelineName}</Text><DefaultButton text="Remove" onClick={() => deleteMut.mutate(e.relationshipId)} /></Stack>)}
          {!data?.upstream.length && <Text>None</Text>}
          <Text variant="large">Downstream</Text>
          {data?.downstream.map(e => <Stack horizontal key={e.relationshipId} horizontalAlign="space-between"><Text>{e.relatedPipelineName}</Text><DefaultButton text="Remove" onClick={() => deleteMut.mutate(e.relationshipId)} /></Stack>)}
          {!data?.downstream.length && <Text>None</Text>}
        </Stack>
      )}
    </Panel>
  );
};
export default LineagePanel;
