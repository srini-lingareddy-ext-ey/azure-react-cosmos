import { FC, useState } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, Spinner, Stack, Text, Dropdown, IDropdownOption, Panel, PanelType, TextField, PrimaryButton } from '@fluentui/react';
import { useFingerprintAuditTrail, useResetBaseline } from './hooks/useFingerprinting';
import type { FingerprintAuditEntryView } from './fingerprintingService';

const ccOpts: IDropdownOption[] = [
  { key: '', text: 'All' },
  { key: 'unauthorized', text: 'Unauthorized' },
  { key: 'authorized', text: 'Authorized' },
];

const ccColor = (cc: string): string | undefined => cc.toLowerCase() === 'unauthorized' ? '#d13438' : cc.toLowerCase() === 'authorized' ? '#107c10' : undefined;

const FingerprintAuditTrailPage: FC = () => {
  const [filter, setFilter] = useState('');
  const { data, isLoading, isError, refetch } = useFingerprintAuditTrail(filter || undefined);
  const [selected, setSelected] = useState<FingerprintAuditEntryView | null>(null);
  const [justification, setJustification] = useState('');
  const resetMutation = useResetBaseline();

  const columns: IColumn[] = [
    { key: 'name', name: 'Artifact', fieldName: 'artifactName', minWidth: 160 },
    { key: 'at', name: 'Detected At', minWidth: 160, onRender: (i: FingerprintAuditEntryView) => <Text>{i.detectedAt ? new Date(i.detectedAt).toLocaleString() : '-'}</Text> },
    { key: 'by', name: 'Changed By', fieldName: 'changedBy', minWidth: 100 },
    { key: 'cc', name: 'Classification', minWidth: 120, onRender: (i: FingerprintAuditEntryView) => <Text styles={{ root: { color: ccColor(i.changeClassification), fontWeight: 600 } }}>{i.changeClassification}</Text> },
    { key: 'sync', name: 'Synced', minWidth: 60, onRender: (i: FingerprintAuditEntryView) => <Text>{i.syncedToImmutableStorage ? 'Yes' : 'No'}</Text> },
  ];

  const handleReset = () => {
    if (!selected || !justification.trim()) return;
    resetMutation.mutate({ artifactId: selected.artifactId, justification }, { onSuccess: () => { setSelected(null); setJustification(''); } });
  };

  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Fingerprint Audit Trail</Text>
      <Dropdown label="Classification" options={ccOpts} selectedKey={filter} onChange={(_, o) => setFilter((o?.key as string) ?? '')} styles={{ root: { maxWidth: 200 } }} />
      {isLoading ? <Spinner /> : isError ? (
        <Stack horizontal tokens={{ childrenGap: 8 }}><Text>Error loading audit trail.</Text><button onClick={() => refetch()}>Retry</button></Stack>
      ) : !data?.length ? <Text>No audit entries found.</Text> : (
        <DetailsList items={data} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden}
          onItemInvoked={(item: FingerprintAuditEntryView) => setSelected(item)} getKey={(i: FingerprintAuditEntryView) => i.id} />
      )}
      <Panel isOpen={!!selected} onDismiss={() => setSelected(null)} type={PanelType.medium} headerText="Audit Entry Detail">
        {selected && (
          <Stack tokens={{ childrenGap: 12 }} styles={{ root: { padding: '16px 0' } }}>
            <Text><b>Artifact:</b> {selected.artifactName}</Text>
            <Text><b>Before Hash:</b> {selected.beforeHash}</Text>
            <Text><b>After Hash:</b> {selected.afterHash}</Text>
            <Text><b>Changed By:</b> {selected.changedBy}</Text>
            <Text><b>Classification:</b> <span style={{ color: ccColor(selected.changeClassification) }}>{selected.changeClassification}</span></Text>
            {selected.approvedWindowName && <Text><b>Approved Window:</b> {selected.approvedWindowName}</Text>}
            <Stack tokens={{ childrenGap: 8 }} styles={{ root: { marginTop: 16 } }}>
              <Text variant="mediumPlus"><b>Reset Baseline</b></Text>
              <TextField label="Justification" value={justification} onChange={(_, v) => setJustification(v ?? '')} required errorMessage={justification.trim() === '' && resetMutation.isError ? 'Justification is required for baseline reset' : undefined} />
              <PrimaryButton text="Reset Baseline" onClick={handleReset} disabled={!justification.trim()} />
            </Stack>
          </Stack>
        )}
      </Panel>
    </Stack>
  );
};

export default FingerprintAuditTrailPage;
