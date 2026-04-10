import { FC } from 'react';
import { Panel, PanelType, Spinner, Stack, Text } from '@fluentui/react';
import { useSLADrillDown } from './hooks/useSLA';
import type { SLADrillDownPipeline, SLADrillDownExecution } from './slaService';

interface SLADrillDownProps {
  businessPlan: string | null;
  onDismiss: () => void;
}

const badgeColor = (status: string): string => {
  const lower = status.toLowerCase();
  if (lower === 'met') return 'green';
  if (lower === 'breached') return 'red';
  if (lower === 'atrisk') return '#c19c00';
  return '#999';
};

const SLADrillDown: FC<SLADrillDownProps> = ({ businessPlan, onDismiss }) => {
  const { data, isLoading, isError, refetch } = useSLADrillDown(businessPlan ?? '', 30);

  return (
    <Panel
      isOpen={!!businessPlan}
      onDismiss={onDismiss}
      type={PanelType.large}
      headerText={`SLA Drill-Down: ${businessPlan ?? ''}`}
    >
      <Stack tokens={{ childrenGap: 16 }} styles={{ root: { padding: '16px 0' } }}>
        {isLoading ? (
          <Spinner label="Loading drill-down..." />
        ) : isError ? (
          <Stack horizontal tokens={{ childrenGap: 8 }}>
            <Text>Error loading drill-down data.</Text>
            <button onClick={() => refetch()}>Retry</button>
          </Stack>
        ) : !data?.length ? (
          <Text>No pipelines found for this business plan.</Text>
        ) : (
          data.map((pipeline: SLADrillDownPipeline) => (
            <Stack key={pipeline.pipelineId} tokens={{ childrenGap: 6 }}>
              <Text variant="mediumPlus" styles={{ root: { fontWeight: 600 } }}>
                {pipeline.pipelineName}
              </Text>
              <Stack horizontal tokens={{ childrenGap: 3 }} wrap>
                {pipeline.executions.map((exec: SLADrillDownExecution) => (
                  <div
                    key={exec.executionId}
                    title={`${exec.status} — ${new Date(exec.startedAt).toLocaleString()}`}
                    style={{
                      width: 14,
                      height: 14,
                      borderRadius: 2,
                      backgroundColor: badgeColor(exec.status),
                    }}
                  />
                ))}
              </Stack>
              <Stack horizontal tokens={{ childrenGap: 12 }}>
                <Stack horizontal tokens={{ childrenGap: 4 }} verticalAlign="center">
                  <div style={{ width: 10, height: 10, borderRadius: 2, backgroundColor: 'green' }} />
                  <Text variant="small">Met</Text>
                </Stack>
                <Stack horizontal tokens={{ childrenGap: 4 }} verticalAlign="center">
                  <div style={{ width: 10, height: 10, borderRadius: 2, backgroundColor: 'red' }} />
                  <Text variant="small">Breached</Text>
                </Stack>
                <Stack horizontal tokens={{ childrenGap: 4 }} verticalAlign="center">
                  <div style={{ width: 10, height: 10, borderRadius: 2, backgroundColor: '#c19c00' }} />
                  <Text variant="small">At Risk</Text>
                </Stack>
              </Stack>
            </Stack>
          ))
        )}
      </Stack>
    </Panel>
  );
};

export default SLADrillDown;
