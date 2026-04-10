import { FC, useMemo, useState } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, Spinner, Stack, Text, DefaultButton } from '@fluentui/react';
import { useEventLog } from './hooks/useEventLog';
import EventLogFilterBar from './EventLogFilterBar';
import EventDetailPanel from './EventDetailPanel';
import type { EventLogEntry } from './eventLogService';

const classificationColor = (c: string): string | undefined => {
  const lower = c.toLowerCase();
  if (lower === 'incident') return '#d13438';
  if (lower === 'alert') return '#c19c00';
  if (lower === 'availabilityissue') return '#e81123';
  if (lower === 'slabreach') return '#e81123';
  return undefined;
};

const EventLogPage: FC = () => {
  const [classification, setClassification] = useState('');
  const [severity, setSeverity] = useState('');
  const [offset, setOffset] = useState(0);
  const [selectedEventId, setSelectedEventId] = useState<string | null>(null);
  const limit = 50;

  const filters = useMemo(() => ({
    classification: classification || undefined,
    severity: severity || undefined,
    limit,
    offset,
  }), [classification, severity, offset]);

  const { data, isLoading, isError, refetch } = useEventLog(filters);

  const columns: IColumn[] = useMemo(() => [
    { key: 'ts', name: 'Timestamp', minWidth: 160, onRender: (i: EventLogEntry) => <Text>{i.timestamp ? new Date(i.timestamp).toLocaleString() : '-'}</Text> },
    { key: 'type', name: 'Type', fieldName: 'eventType', minWidth: 120 },
    { key: 'sev', name: 'Severity', fieldName: 'severity', minWidth: 80 },
    { key: 'cls', name: 'Classification', minWidth: 120, onRender: (i: EventLogEntry) => <Text styles={{ root: { color: classificationColor(i.classification), fontWeight: 600 } }}>{i.classification}</Text> },
    { key: 'src', name: 'Source', fieldName: 'sourceSystem', minWidth: 100 },
    { key: 'mon', name: 'Monitor', fieldName: 'monitorName', minWidth: 120 },
  ], []);

  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Event Log</Text>
      <EventLogFilterBar classification={classification} severity={severity} onClassificationChange={(v) => { setClassification(v); setOffset(0); }} onSeverityChange={(v) => { setSeverity(v); setOffset(0); }} />
      {isLoading ? <Spinner label="Loading events..." /> : isError ? (
        <Stack horizontal tokens={{ childrenGap: 8 }}><Text>Error loading events.</Text><button onClick={() => refetch()}>Retry</button></Stack>
      ) : !data?.items.length ? <Text>No events match the current filters</Text> : (
        <>
          <DetailsList items={data.items} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden}
            onItemInvoked={(item: EventLogEntry) => setSelectedEventId(item.eventId)} getKey={(i: EventLogEntry) => i.eventId} />
          {data.pagination.hasMore && <DefaultButton text="Load more" onClick={() => setOffset((o) => o + limit)} />}
        </>
      )}
      <EventDetailPanel eventId={selectedEventId} onDismiss={() => setSelectedEventId(null)} />
    </Stack>
  );
};

export default EventLogPage;
