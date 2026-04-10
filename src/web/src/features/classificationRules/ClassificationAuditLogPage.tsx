import { FC, useMemo, useState } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, Spinner, Stack, Text, Dropdown, IDropdownOption } from '@fluentui/react';
import { useNavigate } from 'react-router-dom';
import { useClassificationAuditLog } from './hooks/useClassificationRules';
import type { ClassificationAuditEntry } from './classificationRulesService';

const outcomeOpts: IDropdownOption[] = [
  { key: '', text: 'All Outcomes' },
  { key: 'incident', text: 'Incident' },
  { key: 'alert', text: 'Alert' },
  { key: 'informational', text: 'Informational' },
];

const outcomeColor = (o: string): string | undefined => {
  const lower = o.toLowerCase();
  if (lower === 'incident') return '#d13438';
  if (lower === 'alert') return '#c19c00';
  return undefined;
};

const ClassificationAuditLogPage: FC = () => {
  const navigate = useNavigate();
  const [outcome, setOutcome] = useState('');
  const { data, isLoading, isError, refetch } = useClassificationAuditLog(outcome || undefined);

  const columns: IColumn[] = useMemo(() => [
    { key: 'ts', name: 'Classified At', minWidth: 160, onRender: (i: ClassificationAuditEntry) => <Text>{i.classifiedAt ? new Date(i.classifiedAt).toLocaleString() : '-'}</Text> },
    { key: 'eid', name: 'Event ID', minWidth: 120, onRender: (i: ClassificationAuditEntry) => <Text>{i.eventId.slice(0, 8)}...</Text> },
    { key: 'rule', name: 'Matched Rule', fieldName: 'matchedRuleId', minWidth: 100 },
    { key: 'outcome', name: 'Outcome', minWidth: 80, onRender: (i: ClassificationAuditEntry) => <Text styles={{ root: { color: outcomeColor(i.outcome), fontWeight: 600 } }}>{i.outcome}</Text> },
    { key: 'type', name: 'Event Type', fieldName: 'eventType', minWidth: 120 },
    { key: 'src', name: 'Source', fieldName: 'sourceSystem', minWidth: 100 },
  ], []);

  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Classification Audit Log</Text>
      <Dropdown label="Outcome" options={outcomeOpts} selectedKey={outcome} onChange={(_, o) => setOutcome((o?.key as string) ?? '')} styles={{ root: { maxWidth: 200 } }} />
      {isLoading ? <Spinner /> : isError ? (
        <Stack horizontal tokens={{ childrenGap: 8 }}><Text>Error loading audit log.</Text><button onClick={() => refetch()}>Retry</button></Stack>
      ) : !data?.length ? <Text>No audit entries found.</Text> : (
        <DetailsList items={data} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden}
          onItemInvoked={(item: ClassificationAuditEntry) => navigate(`/events?eventId=${item.eventId}`)} getKey={(i: ClassificationAuditEntry) => i.id} />
      )}
    </Stack>
  );
};

export default ClassificationAuditLogPage;
