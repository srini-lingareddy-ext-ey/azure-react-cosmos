import { FC } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, Spinner, Stack, Text, DefaultButton } from '@fluentui/react';
import { useArtifacts, useTriggerScan } from './hooks/useFingerprinting';
import type { MonitoredArtifactView } from './fingerprintingService';

const statusBadge = (s: string) => s.toLowerCase() === 'deviated' ? '#c19c00' : undefined;

const ArtifactRegistryPage: FC = () => {
  const { data, isLoading, isError, refetch } = useArtifacts();
  const scan = useTriggerScan();

  const columns: IColumn[] = [
    { key: 'name', name: 'Artifact', fieldName: 'artifactName', minWidth: 180 },
    { key: 'type', name: 'Type', fieldName: 'artifactType', minWidth: 120 },
    { key: 'status', name: 'Status', minWidth: 100, onRender: (i: MonitoredArtifactView) => <Text styles={{ root: { color: statusBadge(i.currentStatus), fontWeight: 600 } }}>{i.currentStatus}</Text> },
    { key: 'scanned', name: 'Last Scanned', minWidth: 160, onRender: (i: MonitoredArtifactView) => <Text>{i.lastScannedAt ? new Date(i.lastScannedAt).toLocaleString() : '-'}</Text> },
    { key: 'scan', name: '', minWidth: 100, onRender: (i: MonitoredArtifactView) => <DefaultButton text="Scan now" onClick={() => scan.mutate(i.artifactId)} /> },
  ];

  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Monitored Artifacts</Text>
      {isLoading ? <Spinner /> : isError ? (
        <Stack horizontal tokens={{ childrenGap: 8 }}><Text>Error loading artifacts.</Text><button onClick={() => refetch()}>Retry</button></Stack>
      ) : !data?.length ? <Text>No monitored artifacts registered.</Text> : (
        <DetailsList items={data} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={(i: MonitoredArtifactView) => i.artifactId} />
      )}
    </Stack>
  );
};

export default ArtifactRegistryPage;
